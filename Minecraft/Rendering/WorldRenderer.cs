﻿using Assimp.Unmanaged;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Minecraft.Game;
using Minecraft.Graphics;
using Minecraft.Terrain;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Chunk = Minecraft.Terrain.Chunk;

namespace Minecraft.Render
{
    internal class WorldRenderer : ObservableObject
    {
        public static BlockType? CurrentTarget { get; private set; }
        public Action<int>? RenderDistanceChanged;
        public int RenderDistance { get => renderDistance; set { SetProperty(ref renderDistance, value); OnRenderSizeChanged(); } }
        private int renderDistance;

        private PriorityQueue<Vector2,float> renderQueue;
        private ICamera camera;
        public Shader Shader { get; }
        private IWorld world;
        public WorldRenderer()
        {
            camera = Ioc.Default.GetService<ICamera>();

            renderQueue = new PriorityQueue<Vector2,float>();
            Shader = new Shader(@"..\..\..\Graphics\Shaders\Block\blockVert.glsl", @"..\..\..\Graphics\Shaders\Block\blockFrag.glsl");
            Shader.SetDouble("renderDistance", renderDistance);
        }
        public void OnRenderSizeChanged()
        {
            RenderDistanceChanged?.Invoke(renderDistance);
            Shader.SetDouble("renderDistance", renderDistance);
        }
        public void SetWorld(IWorld world)
        {
            this.world = world;

            if(world.Chunks.Count > 0)
            {
                foreach (var chunk in world.Chunks)
                {
                    chunk.Value.Mesh = new ChunkMesh();
                    AddToQueue(chunk.Key);
                }
            }
        }
        public void AddToQueue(Vector2 toRender)
        {
            renderQueue.Enqueue(toRender, (toRender - camera.Position.Xz / Chunk.Size).Length);
        }
        public void RenderWorld()
        {
            AtlasTexturesData.Atlas.Use();

            foreach (var chunk in world.Chunks.Values)
                if(IsChunkInRange(chunk.Position))
                    chunk.Mesh.RenderSolidMesh(Shader);

            foreach (var chunk in world.Chunks.Values)
                if (IsChunkInRange(chunk.Position))
                    chunk.Mesh.RenderTransparentMesh(Shader);

            RenderSelectedBlockFrame();

            CreateMeshesInQueue(0);
        }
        public void CreateMeshesInQueue(float delta)
        {
            if (renderQueue.Count > 0)
                ChunkMesh.CreateMesh(world, renderQueue.Dequeue());

            if (world.ChunksNeedsToBeRegenerated.Count > 0)
                ChunkMesh.CreateMesh(world, world.ChunksNeedsToBeRegenerated.Dequeue());
        }
        private bool IsChunkInRange(in Vector2 chunkPos)
        {
            float xDistance = Math.Abs(camera.Position.X - chunkPos.X * Chunk.Size);
            float zDistance = Math.Abs(camera.Position.Z - chunkPos.Y * Chunk.Size);

            return xDistance <= renderDistance * Chunk.Size && zDistance <= renderDistance * Chunk.Size;
        }
        private void RenderSelectedBlockFrame()
        {
            var blockHitPos = Ray.Cast(world, out bool hit, out FaceDirection hitFace, out double rayDistance);

            CurrentTarget = world.GetBlock(blockHitPos);

            if (hit)
                LineRenderer.WireWrame(blockHitPos, new Vector3(0.0f));
        }
    }
}
