using System;
using System.ComponentModel;
using System.Threading;

namespace SlideshowScreenSaver.Lib
{
	/// <summary>
	/// 別スレッドで稼働するタイマー
	/// Threading.Timerを使いやすくラップした
	/// </summary>
	public sealed class TimerEx : IDisposable
	{
		#region IDisposable

		private bool _disposed;

		public bool IsDisposed
		{
			get { return _disposed; }
		}

		~TimerEx()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// clean up managed object
					Stop();
				}
				_disposed = true;
			}
		}

		#endregion

		#region Events

		private EventHandlerList _events;

		private EventHandlerList Events
		{
			get
			{
				if (_events == null)
					_events = new EventHandlerList();
				return _events;
			}
		}

		#endregion

		#region Tick

		private static readonly object EventTick = new object();

		/// <summary>
		/// タイマーイベント
		/// </summary>
		public event EventHandler<EventArgs> Tick
		{
			add { Events.AddHandler(EventTick, value); }
			remove { Events.RemoveHandler(EventTick, value); }
		}

		private void OnTick(EventArgs e)
		{
			var handler = Events[EventTick] as EventHandler<EventArgs>;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		#endregion

		#region properties

		/// <summary>
		/// タイマーイベント間隔(ms)
		/// </summary>
		public int Interval { get; set; }

		#endregion

		#region fields

		private Timer _timer;

		private bool _run;

		#endregion

		/// <summary>
		/// タイマーの起動
		/// </summary>
		public void Start()
		{
			if (_timer == null)
			{
				_timer = new Timer(OnTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
				_run = true;
				_timer.Change(Interval, Timeout.Infinite);
			}
		}

		/// <summary>
		/// タイマーの停止
		/// </summary>
		public void Stop()
		{
			_run = false;
			if (_timer != null)
			{
				_timer.Dispose();
				_timer = null;
			}
		}

		/// <summary>
		/// タイマーのイベントハンドラ
		/// OnTickを実行後タイマーがまだ有効であれば、再度タイマーを稼働する
		/// </summary>
		/// <param name="state"></param>
		private void OnTimerCallback(object state)
		{
			OnTick(EventArgs.Empty);

			if (_run && _timer != null)
			{
				_timer.Change(Interval, Timeout.Infinite);
			}
		}
	}
}