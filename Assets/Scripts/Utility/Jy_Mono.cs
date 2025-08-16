using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
namespace Jy_Util{
public class Jy_Mono : MonoBehaviour
{
    public static Jy_Mono instance;
    NoArgumentFun noArgumentFun,noArgumentFun1,noArgumentFun2,noArgumentFun3;
    void Awake()
    {
        instance = this;
    }


    public void Delay(NoArgumentFun fun,float time)
    {
        noArgumentFun=fun;
        Invoke(nameof(Execute),time);
    }
    public void Delay1(NoArgumentFun fun,float time)
    {
        noArgumentFun1=fun;
        Invoke(nameof(Execute1),time);
    }

    public void Delay2(NoArgumentFun fun,float time)
    {
        noArgumentFun2=fun;
        Invoke(nameof(Execute2),time);
    }

    public void Delay3(NoArgumentFun fun,float time)
    {
        noArgumentFun3=fun;
        Invoke(nameof(Execute3),time);
    }


    void Execute()=>noArgumentFun();
    void Execute1()=>noArgumentFun1();
    void Execute2()=>noArgumentFun2();
    void Execute3()=>noArgumentFun3();
}
public class util{

public void NullFun()
{

}
}

[System.Serializable]
public struct Struct_Skin_mat
{
    public Material lit;
    public Material unlit;
}

public delegate void NoArgumentFun();
}