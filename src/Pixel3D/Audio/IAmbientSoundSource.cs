﻿using Pixel3D.Animations;

namespace Pixel3D.Audio
{
    /// <summary>
    /// Objects that can play back ambient audio
    /// </summary>
    public interface IAmbientSoundSource
    {
        AmbientSound AmbientSound { get; }
        Position Position { get; }
        bool FacingLeft { get; }
        AnimationSet AnimationSet { get; }
    }
}
