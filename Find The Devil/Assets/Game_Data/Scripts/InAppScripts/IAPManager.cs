using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class IAPManager : MonoBehaviour, IStoreListener
{
	private PurchaseType currentItemType;
	private string currentPurchaseID = null;
	public InAppProduct[] purchaseIDController;
	private void Awake()
	{
		if(m_instance) return;
		m_instance = this;
	}
	private void Start()
	{
		DontDestroyOnLoad(gameObject);
		var module = StandardPurchasingModule.Instance();
		var builder = ConfigurationBuilder.Instance(module);

		string receipt = builder.Configure<IAppleConfiguration>().appReceipt;
		//		In App Purchases may be restricted in a device’s settings, which can be checked for as follows:
		bool canMakePayments = builder.Configure<IAppleConfiguration>().canMakePayments;
		foreach (InAppProduct pIDC in purchaseIDController)
		{
			builder.AddProduct(pIDC.purchaseID, pIDC.purchaseableType);
		}

		UnityPurchasing.Initialize(this, builder);
		for (int i = 0; i < purchaseIDController.Length; i++)
		{
			purchaseIDController[i].localPrice = purchaseIDController[i].price;
		}
	}
	public enum State { PendingInitialize, Initializing, SuccessfullyInitialized, FailedToInitialize };

	private static IAPManager m_instance = null;
	public static IAPManager Instance
	{
		get
		{
			if( m_instance == null )
				m_instance = new IAPManager();

			return m_instance;
		}
	}

	private State m_initializationState = State.PendingInitialize;
	public State InitializationState { get { return m_initializationState; } }
	public bool IsInitialized { get { return m_initializationState == State.SuccessfullyInitialized; } }
	private bool IsProductInitialized()
	{
		return storeController != null && storeExtensions != null;
	}

	public delegate void InitializationCallback( bool success );
	private InitializationCallback m_onInitialized;
	public event InitializationCallback OnInitialized
	{
		add
		{
			if( m_initializationState == State.SuccessfullyInitialized || m_initializationState == State.FailedToInitialize )
				value?.Invoke( m_initializationState == State.SuccessfullyInitialized );
			else
				m_onInitialized += value;
		}
		remove { m_onInitialized -= value; }
	}

	public delegate void CompletedPurchaseCallback( Product product );
	public CompletedPurchaseCallback OnPurchaseCompleted;

	public delegate void FailedPurchaseCallback( Product product, PurchaseFailureReason failureReason );
	public FailedPurchaseCallback OnPurchaseFailed;

	public delegate void NativeIAPWindowClosedCallback();
	private NativeIAPWindowClosedCallback onIAPWindowClosed;

	public delegate void NativeRestoreWindowClosedCallback( bool success );
	private NativeRestoreWindowClosedCallback onRestoreWindowClosed;

	private IStoreController storeController;
	private IExtensionProvider storeExtensions;
#pragma warning disable IDE0044
	private CrossPlatformValidator purchaseValidator;
#pragma warning restore IDE0044

	public void Initialize()
	{
		Initialize( null, true );
	}

	public void Initialize( params ProductDefinition[] products )
	{
		Initialize( products, false );
	}

	public void Initialize( IEnumerable<ProductDefinition> products )
	{
		Initialize( products, false );
	}

	private void Initialize( IEnumerable<ProductDefinition> products, bool initializeWithIAPCatalog )
	{
		if( m_initializationState != State.PendingInitialize )
		{
			Debug.LogWarning( "IAP is already initializing!" );
			return;
		}

#if UNITY_EDITOR
		// Allows simulating failed IAP transactions in the Editor
		StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif

		ConfigurationBuilder builder = ConfigurationBuilder.Instance( StandardPurchasingModule.Instance() );
		 if( initializeWithIAPCatalog )
			 IAPConfigurationHelper.PopulateConfigurationBuilder( ref builder, ProductCatalog.LoadDefaultCatalog() );
		else if( products != null )
			builder.AddProducts( products );
		
		if( StandardPurchasingModule.Instance().appStore == AppStore.GooglePlay )
			builder.Configure<IGooglePlayConfiguration>().SetDeferredPurchaseListener( OnDeferredPurchase );

		m_initializationState = State.Initializing;
		UnityPurchasing.Initialize( this, builder );
	}

	public void Purchase( string productID, NativeIAPWindowClosedCallback onIAPWindowClosed = null )
	{
		if( !IsInitialized )
		{
			Debug.LogWarning( "IAP isn't initialized yet, can't purchased items!" );
			onIAPWindowClosed?.Invoke();

			return;
		}

		this.onIAPWindowClosed = onIAPWindowClosed;
		storeController.InitiatePurchase( productID );
	}

	public void RestorePurchases( NativeRestoreWindowClosedCallback onRestoreWindowClosed = null )
	{
		if( !IsInitialized )
		{
			Debug.LogWarning( "IAP isn't initialized yet, can't restore purchases!" );
			onRestoreWindowClosed?.Invoke( false );

			return;
		}

		this.onRestoreWindowClosed = onRestoreWindowClosed;

		switch( StandardPurchasingModule.Instance().appStore )
		{
			case AppStore.AppleAppStore: storeExtensions.GetExtension<IAppleExtensions>().RestoreTransactions( ( success ) => OnNativeRestoreWindowClosed( success ) ); break;
			case AppStore.GooglePlay: storeExtensions.GetExtension<IGooglePlayStoreExtensions>().RestoreTransactions( ( success ) => OnNativeRestoreWindowClosed( success ) ); break;
		}
	}

	public bool IsNonConsumablePurchased( string productID )
	{
		if( !IsInitialized )
		{
			Debug.LogWarning( "IAP isn't initialized yet, can't check previous purchases!" );
			return false;
		}

		if( string.IsNullOrEmpty( productID ) )
		{
			Debug.LogWarning( "Empty productID is passed!" );
			return false;
		}

		Product product = storeController.products.WithID( productID );
		if( product == null )
		{
			Debug.LogWarning( "IAP Product not found: " + productID );
			return false;
		}

		return product.hasReceipt && IsPurchaseValid( product );
	}

	void IStoreListener.OnInitialized( IStoreController storeController, IExtensionProvider storeExtensions )
	{
		this.storeController = storeController;
		this.storeExtensions = storeExtensions;

		if( StandardPurchasingModule.Instance().appStore == AppStore.AppleAppStore )
			storeExtensions.GetExtension<IAppleExtensions>().RegisterPurchaseDeferredListener( OnDeferredPurchase );

		// The CrossPlatform validator only supports Google Play and Apple App Store
		switch( StandardPurchasingModule.Instance().appStore )
		{
			case AppStore.GooglePlay:
			case AppStore.AppleAppStore:
			case AppStore.MacAppStore:
			{
#if !UNITY_EDITOR
				/*//byte[] appleTangleData = AppleStoreKitTestTangle.Data(); // While testing with StoreKit Testing
				byte[] appleTangleData = AppleTangle.Data();
				purchaseValidator = new CrossPlatformValidator( GooglePlayTangle.Data(), appleTangleData, Application.identifier );*/
#endif
				break;
			}
		}

		m_initializationState = State.SuccessfullyInitialized;
		m_onInitialized?.Invoke( true );
		for (int i = 0; i < purchaseIDController.Length; i++)
		{
			purchaseIDController[i].price = GetPrice(purchaseIDController[i].purchaseID);
			purchaseIDController[i].localPrice = GetPrice(purchaseIDController[i].purchaseID);
		}
	}

	void IStoreListener.OnInitializeFailed( InitializationFailureReason error )
	{
		Debug.LogWarning( "IAP initialization failed: " + error );

		m_initializationState = State.FailedToInitialize;
		m_onInitialized?.Invoke( false );
	}

	public void OnInitializeFailed(InitializationFailureReason error, string message)
	{
		//throw new NotImplementedException();
		print("Not Implemented");
	}

	PurchaseProcessingResult IStoreListener.ProcessPurchase( PurchaseEventArgs purchaseEvent )
	{
		try
		{
			Product product = purchaseEvent.purchasedProduct;
			if( IsPurchaseValid( product ) )
			{
				if( StandardPurchasingModule.Instance().appStore == AppStore.GooglePlay && storeExtensions.GetExtension<IGooglePlayStoreExtensions>().IsPurchasedProductDeferred( product ) )
				{
					// The purchase is deferred; therefore, we do not unlock the content or complete the transaction.
					// ProcessPurchase will be called again once the purchase is completed
					return PurchaseProcessingResult.Pending;
				}

				GameManager.Instance.RemoveAds(currentItemType);
				GameManager.Instance.shopManager.UnlockAllVIPItems(currentItemType);
				GameManager.Instance.uiManager.UnlockAllItems(currentItemType);
				
	//			EventsManager.InAppItemBought(currentItemType);       
				OnPurchaseCompleted?.Invoke( product);
				print(currentItemType+" Item Bought");
			}
			return PurchaseProcessingResult.Complete;
		}
		finally
		{
			OnNativeIAPWindowClosed();
		}
	}

	void IStoreListener.OnPurchaseFailed( Product product, PurchaseFailureReason failureReason )
	{
		Debug.LogWarning( $"IAP purchase failed for '{product.definition.id}': {failureReason}" );

		OnPurchaseFailed?.Invoke( product, failureReason );
		OnNativeIAPWindowClosed();
	}

	private void OnDeferredPurchase( Product product )
	{
		Debug.Log( $"IAP purchase of {product.definition.id} is deferred" );
		OnNativeIAPWindowClosed();
	}

	private bool IsPurchaseValid( Product product )
	{
		if( purchaseValidator != null )
		{
			try
			{
				purchaseValidator.Validate( product.receipt );
			}
			catch( IAPSecurityException reason )
			{
				Debug.LogWarning( "Invalid IAP receipt: " + reason );
				return false;
			}
		}

		return true;
	}

	private void OnNativeIAPWindowClosed()
	{
		try
		{
			onIAPWindowClosed?.Invoke();
			onIAPWindowClosed = null;
		}
		catch( Exception e )
		{
			Debug.LogException( e );
		}
	}

	private void OnNativeRestoreWindowClosed( bool success )
	{
		Debug.Log( "IAP purchases restored: " + success );

		try
		{
			onRestoreWindowClosed?.Invoke( success );
			onRestoreWindowClosed = null;
		}
		catch( Exception e )
		{
			Debug.LogException( e );
		}
	}
	
	public void BuyProductID(string productId, PurchaseType PurchaseType)
	{
		currentItemType = PurchaseType;
		currentPurchaseID = productId;
		Debug.Log("Purchasing id received is " + productId);
		Debug.Log(string.Format("Purchasing product id is ", productId));
		// If Purchasing has been initialized ...
		if (IsProductInitialized())
		{
			// ... look up the Product reference with the general product identifier and the Purchasing 
			// system's products collection.

			//for Cash



			Product product = storeController.products.WithID(productId);

			// If the look up found a product for this device's store and that product is ready to be sold ... 
			if (product != null && product.availableToPurchase)
			{
				Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
				// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
				// asynchronously.
				storeController.InitiatePurchase(product);
			}
			// Otherwise ...
			else
			{
				// ... report the product look-up failure situation  
				Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			}
		}
		// Otherwise ...
		else
		{
			// ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
			// retrying initiailization.
			Debug.Log("BuyProductID FAIL. Not initialized.");
		}
	}
	public void InAppCaller(PurchaseType productType)  //todo: Method to use for purchasing
	{
		for (var i = 0; i < purchaseIDController.Length; i++)
		{
			if (purchaseIDController[i].itemType == productType)
			{
				BuyProductID(purchaseIDController[i].purchaseID, productType);
				break;
			}
		}
	}

	private void ProductPurchaseCompleted(Product product)
	{
		//Product product = storeController.products.WithID(productId);
		//var productId = storeController.products.WithID()
	}
	public string GetPrice(string productID)
	{
		return storeController.products.WithID(productID).metadata.localizedPriceString;
	}
}