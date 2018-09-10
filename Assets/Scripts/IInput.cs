using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInput {

    float Horizontal { get; }

    float Vertical { get; }

    Vector3 MouseEulers { get; }

    int Jump { get; }

    bool Run { get; }

    bool Shoot { get; }

    bool Interact { get; }

}
