#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class SeriousBacktester : Strategy
	{
		
		private RSI rsi;
		private Bollinger bb;
//		private BBW bbw;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"https://www.youtube.com/watch?v=6CdS2FCb29I";
				Name										= "SeriousBacktester";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 600;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				RsiPeriods					= 13;
				RsiUpper					= 75;
				RsiLower					= 20;
				BBLength					= 30;
				BBStdv					= 2;
				BBWLength					= 30;
				BBWStdv					= 2;
				ATRLength					= 14;
				BBWTrigger					= 0.8;
				BBWHourTrigger					= 0.2;
				StopLossPerATR					= 1.6;
				TakeProfitPerATR					= 1.6;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			
			//Logica
			if(Position.MarketPosition == MarketPosition.Flat){
				
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RsiPeriods", Order=1, GroupName="Parameters")]
		public int RsiPeriods
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RsiUpper", Order=2, GroupName="Parameters")]
		public int RsiUpper
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RsiLower", Order=3, GroupName="Parameters")]
		public int RsiLower
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BBLength", Order=4, GroupName="Parameters")]
		public int BBLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BBStdv", Order=5, GroupName="Parameters")]
		public int BBStdv
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BBWLength", Order=6, GroupName="Parameters")]
		public int BBWLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BBWStdv", Order=7, GroupName="Parameters")]
		public int BBWStdv
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ATRLength", Order=8, GroupName="Parameters")]
		public int ATRLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="BBWTrigger", Order=9, GroupName="Parameters")]
		public double BBWTrigger
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BBWHourTrigger", Order=10, GroupName="Parameters")]
		public double BBWHourTrigger
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="StopLossPerATR", Order=11, GroupName="Parameters")]
		public double StopLossPerATR
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="TakeProfitPerATR", Order=12, GroupName="Parameters")]
		public double TakeProfitPerATR
		{ get; set; }
		#endregion

	}
}
