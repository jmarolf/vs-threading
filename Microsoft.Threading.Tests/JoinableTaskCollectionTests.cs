﻿namespace Microsoft.Threading.Tests {
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows.Threading;

	[TestClass]
	public class JoinableTaskCollectionTests : TestBase {
		private JoinableTaskContext context;
		private JoinableTaskFactory joinableFactory;
		private JoinableTaskCollection joinableCollection;

		private Thread originalThread;

		[TestInitialize]
		public void Initialize() {
			this.context = new JoinableTaskContext();
			this.joinableCollection = this.context.CreateCollection();
			this.joinableFactory = this.context.CreateFactory(this.joinableCollection);
			this.originalThread = Thread.CurrentThread;

			// Suppress the assert dialog that appears and causes test runs to hang.
			Trace.Listeners.OfType<DefaultTraceListener>().Single().AssertUiEnabled = false;
		}

		[TestMethod, Timeout(TestTimeout)]
		public void WaitTillEmptyAlreadyCompleted() {
			var awaiter = this.joinableCollection.WaitTillEmptyAsync().GetAwaiter();
			Assert.IsTrue(awaiter.IsCompleted);
		}

		[TestMethod, Timeout(TestTimeout)]
		public async Task WaitTillEmptyWithOne() {
			var evt = new AsyncManualResetEvent();
			var joinable = this.joinableFactory.RunAsync(async delegate {
				await evt;
			});

			var waiter = this.joinableCollection.WaitTillEmptyAsync();
			Assert.IsFalse(waiter.GetAwaiter().IsCompleted);
			await evt.SetAsync();
			await waiter;
		}

		[TestMethod, Timeout(TestTimeout)]
		public async Task EmptyThenMore() {
			var awaiter = this.joinableCollection.WaitTillEmptyAsync().GetAwaiter();
			Assert.IsTrue(awaiter.IsCompleted);

			var evt = new AsyncManualResetEvent();
			var joinable = this.joinableFactory.RunAsync(async delegate {
				await evt;
			});

			var waiter = this.joinableCollection.WaitTillEmptyAsync();
			Assert.IsFalse(waiter.GetAwaiter().IsCompleted);
			await evt.SetAsync();
			await waiter;
		}
	}
}
