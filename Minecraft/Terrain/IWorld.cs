﻿using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Minecraft.Terrain
{
    internal interface IWorld
    {
        Dictionary<Vector2, IChunk> Chunks { get; set; }
        WorldGenerator? WorldGenerator { get; set; }
        Queue<Vector2> ChunksNeedsToBeRegenerated { get; }

        void AddBlock(Vector3 pos, BlockType block);
        void AddChunk(Vector2 pos, IChunk chunk);
        void AddEntity(Vector3 position, EntityType entityType, IChunk chunk);
        BlockType? GetBlock(Vector3 pos);
        IChunk? GetChunk(Vector3 pos, out Vector2 chunkPos);
        void RemoveBlock(Vector3 pos);
    }
}