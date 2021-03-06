﻿using System;
using SQLite.Net.Attributes;

namespace BodyReportMobile.Core.Models
{
	[Table("BodyExercise")]
	public class BodyExerciseRow
	{
        /// <summary>
        /// Id
        /// </summary>
        [PrimaryKey, Column("Id")]
        public int Id { get; set; }

		/// <summary>
		/// Muscle Id
		/// </summary>
		public int MuscleId { get; set; }
	}
}

