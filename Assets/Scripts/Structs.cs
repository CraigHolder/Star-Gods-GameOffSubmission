using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Structs
{

    public struct CraftingRecipe
    {
        public int metal;
        public int life;
        public int water;
        public int fuel;
        public int energy;

        public GameObject result;

        public CraftingRecipe(int m, int l, int w, int f, int e, GameObject r)
        {
            metal = m;
            life = l;
            water = w;
            fuel = f;
            energy = e;
            result = r;
        }
    }

}
