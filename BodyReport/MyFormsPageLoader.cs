﻿using System;
using MvvmCross.Forms.Presenter.Core;
using System.Reflection;
using MvvmCross.Platform.IoC;
using System.Linq;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Platform;
using System.Collections.Generic;

namespace BodyReport
{
	public class MyFormsPageLoader : MvxFormsPageLoader
	{
		private IMvxViewsContainer _viewFinder;

		public MyFormsPageLoader ()
		{
		}

		/*
		protected override Type GetPageType(string pageName)
		{
			return typeof(BodyReport.Pages.TipPage).GetTypeInfo().Assembly.CreatableTypes().FirstOrDefault(t => t.Name == pageName);
		}*/

		protected override Type GetPageType(MvxViewModelRequest request)
		{
			if (_viewFinder == null)
				_viewFinder = Mvx.Resolve<IMvxViewsContainer> ();

			try
			{
				return _viewFinder.GetViewType (request.ViewModelType);
			}
			catch(KeyNotFoundException) 
			{
				var pageName = GetPageName(request);
				return typeof(BodyReport.Pages.TipPage).GetTypeInfo().Assembly.CreatableTypes().FirstOrDefault(t => t.Name == pageName);


				//return request.ViewModelType.GetTypeInfo().Assembly.CreatableTypes()
				//	.FirstOrDefault(t => t.Name == pageName);
			}
		}
	}
}
