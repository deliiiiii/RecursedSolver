using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ObservableValue<T, TCLASS>
{
    private T value;
    //private readonly string valueType;
    private readonly TCLASS valueClass;
    //delegate void OnValueChangeDelegate(T oldValue, T newValue, string valueType);
    delegate void OnValueChangeDelegate(T oldValue, T newValue, TCLASS valueClass);
    event OnValueChangeDelegate OnValueChangeEvent;
    //public ObservableValue(T value, string valueType)
    public ObservableValue(T value, TCLASS valueClass)
    {
        this.value = value;
        this.valueClass = valueClass;
        this.OnValueChangeEvent += OnValueChange;

    }
    public T Value
    {
        get => value;
        set
        {
            T oldValue = this.value;
            if (this.value.Equals(value))
                return;
            //if (typeof(T) == typeof(int) && (int.Parse(value.ToString()) < 0))
            //    return;

            //if (valueType == 7 && (int.Parse(value.ToString()) < 0))
            //{
            //    T t = (T)(object)Convert.ToInt32(0);
            //    this.value = t;
            //}
            this.value = value;
            OnValueChangeEvent?.Invoke(oldValue, value, this.valueClass);
        }
    }
    public void OnValueChange(T oldValue, T newValue, TCLASS valueClass)
    {
        //if (valueClass is a)
        //    ((a)((object)valueClass)).Func();
    }
}
