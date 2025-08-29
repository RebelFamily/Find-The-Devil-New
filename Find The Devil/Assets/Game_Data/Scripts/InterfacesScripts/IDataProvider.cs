public interface IDataProvider
{
    void Save(string key, string data);
    string Load(string key, string data);
    void Delete(string key); // Optional
    bool HasKey(string key); // Optional
    
    void Save(string key, int data);
    int Load(string key, int data);
}