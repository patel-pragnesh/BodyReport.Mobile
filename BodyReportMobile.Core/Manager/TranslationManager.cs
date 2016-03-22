﻿using System;
using BodyReportMobile.Core.Crud.Module;
using Message;
using System.Collections.Generic;
using SQLite.Net;

namespace BodyReportMobile.Core.Manager
{
	public class TranslationManager : ServiceManager
	{
		TranslationModule _module = null;

		public TranslationManager(SQLiteConnection dbContext) : base(dbContext)
		{
			_module = new TranslationModule(_dbContext);
		}

		internal TranslationVal GetTranslation(TranslationValKey key)
		{
			return _module.Get(key);
		}

		public List<TranslationVal> FindTranslation()
		{
			return _module.Find();
		}

		internal TranslationVal UpdateTranslation(TranslationVal translation)
		{
			return _module.Update(translation);
		}

		internal List<TranslationVal> UpdateTranslationList(List<TranslationVal> translationList)
		{
			List<TranslationVal> list = new List<TranslationVal> ();
			foreach (var translation in translationList)
			{
				list.Add(_module.Update (translation));
			}
			return list;
		}
	}
}

