using System.Collections;
using System.Collections.Generic;
using TwoStickClassicExample;
using UnityEngine;

namespace TwoStickClassicExample
{
    [RequireComponent(typeof(Position2D))]
    [RequireComponent(typeof(Heading2D))]
    public class MoveSpeed : MonoBehaviour
    {

        public float Value;
        
        void Update ()
        {
            //var transform = GetComponent<Transform2D>();
            //transform.Position += transform.Heading * Speed * Time.deltaTime;
            //GetComponent<Position2D>().Value += GetComponent<Heading2D>().Value * Value * Time.deltaTime;
        }
    }
}