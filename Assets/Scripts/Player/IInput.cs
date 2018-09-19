using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInput {

    float Horizontal { get; }

    float Vertical { get; }

    Vector3 MouseEulers { get; }

    bool Jump { get; }

    bool Run { get; }

    bool Shoot { get; }

    bool Aim { get; }

    bool Reload { get; }

    bool Interact { get; }

}
