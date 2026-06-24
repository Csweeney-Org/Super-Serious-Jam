using System;

[Serializable]
public struct BoingData
{
    public float BounceStrength;
    public float HorizontalForce;
    public float RotationForce;
    public bool CanBounce;
    public float Damage;
    public float Weight;


    public BoingData(float bounceStrength, float horizontalForce, float rotationForce, bool canBounce, float damage, float weight)
    {
        BounceStrength = bounceStrength;
        HorizontalForce = horizontalForce;
        RotationForce = rotationForce;
        CanBounce = canBounce;
        Damage = damage;
        Weight = weight;
    }
    public static BoingData WithoutBounce(BoingData oldData)
    {
        return new BoingData(
            oldData.BounceStrength,
            oldData.HorizontalForce,
            oldData.RotationForce,
            canBounce: false,
            oldData.Damage,
            oldData.Weight
            );
    }
}
