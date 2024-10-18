using Godot;
using System;

public static partial class VectorMath
{

    public static Transform3D AlignedToNormal(this Transform3D transform, Vector3 normal)
    {
        transform.Basis.Y = normal;
        transform.Basis.X = -transform.Basis.Z.Cross(normal);
        transform.Basis = transform.Basis.Orthonormalized();

        return transform;
    }
}
