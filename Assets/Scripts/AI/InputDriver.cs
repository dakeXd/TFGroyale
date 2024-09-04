using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputDriver {
    public abstract AiAction ProcessInput(NetworkInput input);
}
