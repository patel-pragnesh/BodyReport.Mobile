﻿using System;
using MvvmCross.Core.ViewModels;
using System.Threading.Tasks;
using BodyReportMobile.Core.Framework;
using BodyReportMobile.Core.MvxMessages;

namespace BodyReportMobile.Core.ViewModels
{
	public class BaseViewModel : MvxViewModel
	{
		private static readonly string TCS_VALUE = "TCS_VALUE";

		private bool _autoClearViewModelDataCollection;

		/// <summary>
		/// The view model GUID.
		/// </summary>
		private string _viewModelGuid;

		/// <summary>
		/// Title of viewmodel
		/// </summary>
		private string _titleLabel = string.Empty;
        
		public BaseViewModel ()
		{
            AppMessenger.AppInstance.Register<MvxMessageFormClosed>(this, OnFormClosedMvxMessage);
            //TODO unscribe
		}

		public virtual void Init(string viewModelGuid, bool autoClearViewModelDataCollection)
		{
			_viewModelGuid = viewModelGuid;
			_autoClearViewModelDataCollection = autoClearViewModelDataCollection;
			InitTranslation ();
		}

		protected virtual void InitTranslation()
		{
		}

		private void OnFormClosedMvxMessage(MvxMessageFormClosed mvxMessageFormClosed)
		{
			if (mvxMessageFormClosed != null && !string.IsNullOrWhiteSpace (mvxMessageFormClosed.ViewModelGuid) && 
				mvxMessageFormClosed.ViewModelGuid == _viewModelGuid) {
				//It's for this view Model
				var tcsShowingViewModel = ViewModelDataCollection.Get<TaskCompletionSource<bool>> (_viewModelGuid, TCS_VALUE);
				if (tcsShowingViewModel != null)
					tcsShowingViewModel.SetResult (!mvxMessageFormClosed.CanceledView);

				if(_autoClearViewModelDataCollection)
					ViewModelDataCollection.Clear (_viewModelGuid);
			}
		}

		protected static async Task<bool> ShowModalViewModel<TViewModel>(BaseViewModel baseMvxViewModel) where TViewModel : MvxViewModel
		{
			string viewModelGuid = Guid.NewGuid ().ToString ();
			return await ShowModalViewModel<TViewModel>(viewModelGuid, true, baseMvxViewModel);
		}

		protected static async Task<bool> ShowModalViewModel<TViewModel>(string viewModelGuid, bool autoClearViewModelDataCollection, BaseViewModel baseMvxViewModel) where TViewModel : MvxViewModel
		{
			var tcs = new TaskCompletionSource<bool>();
			ViewModelDataCollection.Push (viewModelGuid, TCS_VALUE, tcs);
			
			bool result = baseMvxViewModel.ShowViewModel<TViewModel> (new { viewModelGuid = viewModelGuid, autoClearViewModelDataCollection = autoClearViewModelDataCollection});

			if (!result) //Not awaiting because view is not display
				tcs.SetResult (false);
			
			return await tcs.Task;
		}

		protected bool CloseViewModel()
		{
			if (Close (this)) {
                AppMessenger.AppInstance.Send(new MvxMessageFormClosed(ViewModelGuid, false));
				return true;
			}
			return false;
		}

		#region accessor

		public string ViewModelGuid {
			get {
				return _viewModelGuid;
			}
		}

		public string TitleLabel {
			get {
				return _titleLabel;
			}
			set {
				_titleLabel = value;
				RaisePropertyChanged (() => TitleLabel);
			}
		}

		#endregion
	}
}

