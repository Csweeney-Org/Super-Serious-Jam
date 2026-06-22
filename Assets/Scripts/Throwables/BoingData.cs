using System;

[Serializable]
public struct BoingData
{
    public float BounceStrength;
    public float HorizontalForce;
    public float RotationForce;
    public bool CanBounce;

    public BoingData(float bounceStrength, float horizontalForce, float rotationForce, bool canBounce)
    {
        BounceStrength = bounceStrength;
        HorizontalForce = horizontalForce;
        RotationForce = rotationForce;
        CanBounce = canBounce;
    }
    public static BoingData WithoutBounce(BoingData oldData)
    {
        return new BoingData(
            oldData.BounceStrength,
            oldData.HorizontalForce,
            oldData.RotationForce,
            canBounce: false
            );
    }
}
