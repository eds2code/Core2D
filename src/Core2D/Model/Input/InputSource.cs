﻿using System;

namespace Core2D.Input
{
    public abstract class InputSource : IInputSource
    {
        public IObservable<InputArgs> BeginDown { get; set; }

        public IObservable<InputArgs> BeginUp { get; set; }

        public IObservable<InputArgs> EndDown { get; set; }

        public IObservable<InputArgs> EndUp { get; set; }

        public IObservable<InputArgs> Move { get; set; }
    }
}
