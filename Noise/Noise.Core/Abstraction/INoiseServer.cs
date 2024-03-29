﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Core.Abstraction
{
    public interface INoiseServer : IDisposable
    {
        Task StartAsync(CancellationToken cancellationToken);
        void Stop();
    }
}
