using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class UpdateFunctions
    {
        // Going to end up maintaining two synchronised lists here.  Not too bad
        // tho as the internals of this class are the only thing that can edit them
        // and its fairly simple.

        List<Func<bool>> updateFunctions_;
        List<TimeSpan> updateFreq_;
        List<TimeSpan> timeOfLastUpdate_;
        TimeSpan timeSinceStart_;
        int index_ = 0;

        public UpdateFunctions()
        {
            updateFreq_ = new List<TimeSpan>();
			updateFunctions_ = new List<Func<bool>>();
            timeOfLastUpdate_ = new List<TimeSpan>();
            timeSinceStart_ = new TimeSpan(0);
        }

        /// <summary>
        /// Add an update function to the list.
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="updateFrequency">How often to call this function</param>
        /// <returns>The index of the function in the internal list.</returns>
		public int AddUpdateFunction(Func<bool> function, TimeSpan updateFrequency)
        {
            updateFunctions_.Add(function);
            updateFreq_.Add(updateFrequency);
            timeOfLastUpdate_.Add(new TimeSpan(0));
            return updateFreq_.Count - 1;
        }

        /// <summary>
        /// Try to remove a function from the update list.
        /// </summary>
        /// <param name="index">The index of the function to remove</param>
        /// <returns>True if removal succeeds, false otherwise</returns>
        public bool RemoveUpdateFunction(int index)
        {
            if (updateFreq_.Count > index)
            {
                updateFunctions_.RemoveAt(index);
                updateFreq_.RemoveAt(index);
                timeOfLastUpdate_.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calls one update function from the list.  Maintains a "next to update" index.
        /// If that function was called too recently it will not do anything.  Increments the
        /// index either way.
        /// </summary>
        /// <param name="timeSinceLastUpdate">The amount of time that has passed since this
        /// function was last called.  NOT since the indexed function was last called.
        /// </param>
        public bool CallUpdateFunction(TimeSpan timeSinceLastUpdate)
        {
            timeSinceStart_ += timeSinceLastUpdate;
			bool shouldExit = false;

            if (updateFreq_.Count > index_)
            {
                TimeSpan timeSinceLastFuncUpdate = timeSinceStart_ - timeOfLastUpdate_[index_];
                if (timeSinceLastFuncUpdate > updateFreq_[index_])
                {
                    shouldExit = updateFunctions_[index_]();
                    timeOfLastUpdate_[index_] = timeSinceStart_;
                }
            }
            index_++;
            if (index_ >= updateFreq_.Count)
            {
                index_ = 0;
            }

			return shouldExit;
        }
    }
}
