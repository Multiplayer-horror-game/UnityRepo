using System.Collections.Generic;
using UnityEngine;

namespace Fase1.Math
{
    
    public class NatureNoiseGenerator : NoiseGenerator
    {
        public NatureNoiseGenerator(float scale, int xOffset, int yOffset, int vertices, float heightMultiplier, float heightOffset) : base(scale, xOffset, yOffset, vertices, heightMultiplier, heightOffset)
        {
            this.vertices = vertices;
            this.yOffset = yOffset;
            this.xOffset = xOffset;
            this.Scale = scale;
            this.heightMultiplier = heightMultiplier;
            this.heightOffset = heightOffset;
        }
        
        //
        public Dictionary<Vector3, float> GenerateObjectOffsets(int xChunk, int yChunk, float physicalSize)
        {
            //random noise for tree actuation(where to actually place trees and where not <3)
            //float[,] noise = GenerateNoiseChunk(xChunk, yChunk, xOffset + 69420, yOffset + 69420);
            
            //base world noise
            float[,] height = GenerateNoiseChunk(xChunk, yChunk);

            float vDistance = physicalSize / vertices;

            Dictionary<Vector3, float> result = new Dictionary<Vector3, float>();

            for (int x = 0; x < vertices; x++)
            {
                for (int y = 0; y < vertices; y++)
                {
                    result.Add(new Vector3((x + Random.Range(-0.8f,0.8f)) * vDistance,height[x,y] - 2,(y + Random.Range(-0.8f,0.8f)) * vDistance), Random.Range(0,359));
                }
            }
            
            return result;
        } 
    }
}