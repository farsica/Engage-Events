﻿// <copyright file="EventCollection.cs" company="Engage Software">
// Engage: Events - http://www.engagemodules.com
// Copyright (c) 2004-2008
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

namespace Engage.Events
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using Data;

    public enum ListingMode
    {
        All,
        CurrentMonth,
        Future,
        Past
    }

    /// <summary>
    /// A strongly-typed collection of <see cref="Event"/> objects.
    /// </summary>
    /// <remarks>
    /// This class inherits from BindingList for future support.
    /// </remarks>
    public class EventCollection : BindingList<Event>
    {
        /// <summary>
        /// Backing field for <see cref="TotalRecords"/>.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int totalRecords;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCollection"/> class.
        /// </summary>
        /// <param name="totalRecords">The total number of records in this collection.  Fills <see cref="TotalRecords"/>.</param>
        private EventCollection(int totalRecords)
        {
            this.totalRecords = totalRecords;
        }

        /// <summary>
        /// Gets the total number of events in this collection.
        /// </summary>
        /// <value>The total number of events in this collection.</value>
        public int TotalRecords
        {
            [DebuggerStepThrough]
            get { return totalRecords; }
        }

        /// <summary>
        /// Loads a page of events either for the current month, or all future months.
        /// </summary>
        /// <param name="portalId">The ID of the portal that the events are for.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="index">The index.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="showAll">if set to <c>true</c> [show all].</param>
        /// <param name="featuredOnly">if set to <c>true</c> [featured only].</param>
        /// <returns>
        /// A page of events for either this month, or all future months.
        /// </returns>
        public static EventCollection Load(int portalId, ListingMode mode, string sortColumn, int index, int pageSize, bool showAll, bool featuredOnly)
        {
            string storedProcName = "spGetEvents";
     
            IDataProvider dp = DataProvider.Instance;
            try
            {
                using (IDataReader reader = dp.ExecuteReader(
                    CommandType.StoredProcedure,
                    dp.NamePrefix + storedProcName,
                    Utility.CreateIntegerParam("@portalId", portalId),
                    Utility.CreateVarcharParam("@sortColumn", sortColumn, 200),
                    Utility.CreateVarcharParam("@listingMode", mode.ToString(), 20),
                    Utility.CreateIntegerParam("@index", index),
                    Utility.CreateIntegerParam("@pageSize", pageSize),
                    Utility.CreateBitParam("@showAll", showAll),
                    Utility.CreateBitParam("@featured", featuredOnly))
                    )
                {
                    return FillEvents(reader);
                }
            }
            catch (Exception exc)
            {
                throw new DBException(storedProcName, exc);
            }
        }

        /// <summary>
        /// Fills a collection of events from a <see cref="DataSet"/>.
        /// </summary>
        /// <param name="reader">
        /// An un-initialized data reader with two records.  
        /// The first should be a single integer, representing the total number of events (non-paged) for the requested query.
        /// The second should be a collection of records representing the events requested.
        /// </param>
        /// <returns>A collection of instantiated <see cref="Event"/> object, as represented in <paramref name="reader"/>.</returns>
        private static EventCollection FillEvents(IDataReader reader)
        {
            if (reader.Read())
            {
                //there are cases in the UI where we need to know what the total number of records are.
                int totalRecords = (int)reader["TotalRecords"];
                EventCollection events = new EventCollection(totalRecords);

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        events.Add(Event.Fill(reader, totalRecords));
                    }
                    return events;
                }
            }
            throw new DBException("Data reader did not have the expected structure.  An error must have occurred in the query.");
        }
    }
}