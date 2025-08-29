using UnityEngine;

public interface IWeapon
{
    void Fire();
    void Fire(Vector3 points);
    float GetDamage();
    public void HandleTapInput(Vector3 pos);
    void Activate();
    void Deactivate();
    void Init();
    void FireAtCharacter(CharacterReactionHandler characterReactionHandler);
}