﻿using Pixel3D.Animations;
using Pixel3D.Engine;
using Pixel3D.Engine.Levels;

namespace Pixel3D.Levels
{
    public abstract class LevelSubBehaviour : ILevelSubBehaviour
    {
        public virtual void BeforeBeginLevel(UpdateContext updateContext) { }
        public virtual void BeginLevelStoryTriggers(UpdateContext updateContext) { }
        public virtual void BeginLevel(UpdateContext updateContext, Level previousLevel, string targetSpawn) { }
        public virtual void BeforeUpdate(UpdateContext updateContext) { }
        public virtual void AfterUpdate(UpdateContext updateContext) { }
        public virtual void BeforeBackgroundDraw(DrawContext drawContext) { }
        public virtual void AfterDraw(DrawContext drawContext) { }
        
        public virtual void PlayerDidLeave(UpdateContext updateContext, int playerIndex) { }
        public virtual void PlayerDidJoin(UpdateContext updateContext, int playerIndex) { }

        public virtual void LevelWillChange(UpdateContext updateContext, LevelBehaviour nextLevelBehaviour, Level nextLevel) { }
    }
}