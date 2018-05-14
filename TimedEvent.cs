﻿namespace MultiAnnouncer
{
    using System;
    using System.Collections.Generic;
    using System.Timers;

    public class TimedEvent
    {

        private Dictionary<string, object> _args;
        private readonly System.Timers.Timer _timer;
        private long lastTick;
        public static int _elapsedCount;

        public delegate void TimedEventFireDelegate(TimedEvent evt);

        public event TimedEventFireDelegate OnFire;

        public TimedEvent(double interval)
        {
            this._timer = new System.Timers.Timer();
            this._timer.Interval = interval;
            this._timer.Elapsed += new ElapsedEventHandler(this._timer_Elapsed);
            _elapsedCount = 0;
        }

        public TimedEvent(double interval, Dictionary<string, object> args)
            : this(interval)
        {
            this.Args = args;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.OnFire != null)
            {
                this.OnFire(this);
            }

            _elapsedCount += 1;
            this.lastTick = DateTime.UtcNow.Ticks;
        }

        public void Start()
        {
            this._timer.Start();
            this.lastTick = DateTime.UtcNow.Ticks;
        }

        public void Stop()
        {
            this._timer.Stop();
        }

        public void Kill()
        {
            this._timer.Stop();
            this._timer.Dispose();
        }

        public Dictionary<string, object> Args
        {
            get { return this._args; }
            set { this._args = value; }
        }

        public double Interval
        {
            get { return this._timer.Interval; }
            set { this._timer.Interval = value; }
        }

        public double TimeLeft
        {
            get { return (this.Interval - ((DateTime.UtcNow.Ticks - this.lastTick) / 0x2710L)); }
        }

        public int ElapsedCount
        {
            get { return _elapsedCount; }
        }
    }
}