﻿using System;
using SQLite.Net;
using BodyReport.Message;
using BodyReportMobile.Core.Crud.Transformer;
using System.Collections.Generic;
using BodyReportMobile.Core.Models;

namespace BodyReportMobile.Core.Crud.Module
{
	public class MuscleModule : Crud
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="dbContext">database context</param>
		public MuscleModule(SQLiteConnection dbContext) : base(dbContext)
		{
		}
        
		/// <summary>
		/// Create data in database
		/// </summary>
		/// <param name="muscle">Data</param>
		/// <returns>insert data</returns>
		public Muscle Create(Muscle muscle)
		{
			if (muscle == null || muscle.Id == 0)
				return null;

			var row = new MuscleRow();
			MuscleTransformer.ToRow(muscle, row);
			_dbContext.Insert(row);

			return MuscleTransformer.ToBean(row);
		}

		/// <summary>
		/// Get data in database
		/// </summary>
		/// <param name="key">Primary Key</param>
		/// <returns>read data</returns>
		public Muscle Get(MuscleKey key)
		{
			if (key == null || key.Id == 0)
				return null;

			var row = _dbContext.Table<MuscleRow>().Where(m => m.Id == key.Id).FirstOrDefault();
			if (row != null)
			{
				return MuscleTransformer.ToBean(row);
			}
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public List<Muscle> Find(MuscleCriteria muscleCriteria = null)
		{
			List<Muscle> resultList = null;
			TableQuery<MuscleRow> rowList = _dbContext.Table<MuscleRow>();
			CriteriaTransformer.CompleteQuery(ref rowList, muscleCriteria);

			if (rowList != null && rowList.Count() > 0)
			{
				resultList = new List<Muscle>();
				foreach (var row in rowList)
				{
					resultList.Add(MuscleTransformer.ToBean(row));
				}
			}
			return resultList;
		}

		/// <summary>
		/// Update data in database
		/// </summary>
		/// <param name="muscle">data</param>
		/// <returns>updated data</returns>
		public Muscle Update(Muscle muscle)
		{
			if (muscle == null || muscle.Id == 0)
				return null;

			var row = _dbContext.Table<MuscleRow>().Where(m => m.Id == muscle.Id).FirstOrDefault();
			if (row == null)
			{ // No data in database
				return Create(muscle);
			}
			else
			{ //Modify Data in database
				MuscleTransformer.ToRow(muscle, row);
                _dbContext.Delete(row); //Update don't work... need delete and insert
                _dbContext.Insert(row);
                return MuscleTransformer.ToBean(row);
			}
		}

		/// <summary>
		/// Delete data in database
		/// </summary>
		/// <param name="key">Primary Key</param>
		public void Delete(MuscleKey key)
		{
			if (key == null || key.Id == 0)
				return;

			var row = _dbContext.Table<MuscleRow>().Where(m => m.Id == key.Id).FirstOrDefault();
			if (row != null)
			{
				_dbContext.Delete(row);
			}
		}
	}
}

