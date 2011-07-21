/*****************************************************************************************
 *  p2p-player
 *  An audio player developed in C# based on a shared base to obtain the music from.
 * 
 *  Copyright (C) 2010-2011 Dario Mazza, Sebastiano Merlino
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *  
 *  Dario Mazza (dariomzz@gmail.com)
 *  Sebastiano Merlino (etr@pensieroartificiale.com)
 *  Full Source and Documentation available on Google Code Project "p2p-player", 
 *  see <http://code.google.com/p/p2p-player/>
 *
 ******************************************************************************************/

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TransportService
{
    /// <summary>
    /// This class represents a single object of the ThreadPool. It contains a Thread using the paradigm
    /// AsOne in order to hide proper methods of a thread class.
    /// Each thread has a condition state that indicates wheather the thread is busy or free.
    /// </summary>
    /// <seealso cref="TransportService.ThreadPool"/>
    internal class ThreadPoolObject
    {
        /// <summary>
        /// Enumeration containing all possible state that a thread can assume.
        /// A thread can be FREE or BUSY. A free thread can be immediatly used. A BUSY thread put the requestor
        /// in wait until the thread finish its previuos execution.
        /// </summary>
        public enum ThreadState { FREE, BUSY };

        /// <summary>
        /// Default construtor. It is used to create a new ThreadPoolObject and to initialize the internal
        /// AutoResetEvent.
        /// </summary>
        public ThreadPoolObject()
        {
            this.ThreadLock = new AutoResetEvent(true);
        }

#region Properties
        /// <summary>
        /// Property representing the internal AutoResetEvent used to lock the thread execution
        /// </summary>
        public AutoResetEvent ThreadLock
        {
            set;
            get;
        }

        /// <summary>
        /// Property representing the state of the thread referring to the enumeration.
        /// </summary>
        public ThreadState State
        {
            get;
            set;
        }
#endregion

        /// <summary>
        /// Private method used to run a delegate passed. This method is passed (with irony as a Delegate)
        /// to the internal thread of the class that executes it and so the delegate passed. After the execution
        /// is terminated the thread is put in FREE state and the ResetEvent is set.
        /// </summary>
        /// <param name="d">A delegate to execute</param>
        private void assignAndStartDelegate(Delegate d)
        {
            d.DynamicInvoke();
            this.State = ThreadState.FREE;
            this.ThreadLock.Set();
        }

        /// <summary>
        /// Method called by extern requestors to assign a delegate to execute to the class.
        /// This method put the thread in BUSY state and reset the AutoResetEvent. It then start the inner
        /// thread passing using an internal delegate function and passing the delegate to it.
        /// </summary>
        /// <param name="d"></param>
        public void AssignAndStart(Delegate d)
        {
            this.State = ThreadState.BUSY;
            this.ThreadLock.WaitOne();
            this.ThreadLock.Reset();
            Thread t = new Thread(new ThreadStart(() => assignAndStartDelegate(d)));
            t.Start();
        }
    }
}
