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
	public class DotelRenkoSCRIPT : Strategy
	{
		private double BE_TRIGGER;
		private bool BE_ON;

		private RSI RSI1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Dotel Renko With RSI";
				Name										= "DotelRenkoSCRIPT";
				Calculate									= Calculate.OnPriceChange;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
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
                RSI_PERIOS					= 14;
				RSI_UP						= 60;
				RSI_LOW						= 30;
				COMPRAS						= true;
				VENTAS						= true;
				ORDER_SIZE_1				= 1;
				BE_BOOL						= true;
				SL_INICIAL					= -8;
				BE_DISTANCE					= 4;
				BE_OFFSET					= 1;
				TP_INICIAL					= 8;
				SL_INCIAL_VENTA				= 8;
				BE_DISTANCE_VENTA			= -4;
				TP_INICIAL_VENTAS			= -8;
				BE_OFFSET_VENTA				= -1;
				BE_TRIGGER					= 1;
				BE_ON					= false;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				RSI1				= RSI(Close, Convert.ToInt32(RSI_PERIOS), 3);
				RSI1.Plots[0].Brush = Brushes.Snow;
				RSI1.Plots[1].Brush = Brushes.Transparent;
				AddChartIndicator(RSI1);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			 // Set 1
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				BE_ON = false;
			}
			
			if (CurrentBars[0] < 1)
				return;

			 // Compras
			if ((COMPRAS == true)
				 && (CrossAbove(RSI1.Avg, 30, 1))
				 && (Position.MarketPosition == MarketPosition.Flat))
			{
				EnterLong(Convert.ToInt32(ORDER_SIZE_1), @"COMPRA1");
				BE_ON = false;
			}
			
			 // Compras Abiertas BE Setted
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (BE_ON == false))
			{
				ExitLongStopMarket(Convert.ToInt32(ORDER_SIZE_1), (Position.AveragePrice - (SL_INICIAL * TickSize)) , @"STOP_COMPRA1", @"COMPRA1");
				ExitLongLimit(Convert.ToInt32(ORDER_SIZE_1), (Position.AveragePrice + (TP_INICIAL * TickSize)) , @"TP_COMPRA1", @"COMPRA1");
				BE_TRIGGER = (Position.AveragePrice + (BE_DISTANCE * TickSize)) ;
			}
			
			 // Compras BreakEven Activated
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (Close[0] >= BE_TRIGGER))
			{
				BE_ON = true;
				ExitLongStopMarket(Convert.ToInt32(ORDER_SIZE_1), (Position.AveragePrice + (BE_OFFSET * TickSize)) , @"BE_COMPRA1", @"COMPRA1");
				ExitLongLimit(Convert.ToInt32(ORDER_SIZE_1), (Position.AveragePrice + (TP_INICIAL * TickSize)) , @"TP_COMPRA1", @"COMPRA1");
			}

            // Compras SALVAVIDAS
            if ((Position.MarketPosition == MarketPosition.Long)
                && Close[0] <= SL_INICIAL && BE_ON == false)
                {
                    ExitLong(@"COMPRA1");
                }
            if ((Position.MarketPosition == MarketPosition.Long)
                && Close[0] <= BE_OFFSET && BE_ON == true)
            {
                ExitLong(@"COMPRA1");
            }







            // Ventas
            if ((VENTAS == true)
				 && (CrossBelow(RSI1.Avg, 60, 1))
				 && (Position.MarketPosition == MarketPosition.Flat))
			{
				EnterShort(Convert.ToInt32(ORDER_SIZE_1), @"VENTA1");
				BE_ON = false;
			}
			
			 // Ventas Abiertas BE Setted
			if ((Position.MarketPosition == MarketPosition.Short)
				 && (BE_ON == false))
			{
				ExitShortStopMarket(Convert.ToInt32(ORDER_SIZE_1), (Position.AveragePrice + (SL_INCIAL_VENTA * TickSize)) , @"SL_VENTA1", @"VENTA1");
				ExitLongLimit(Convert.ToInt32(ORDER_SIZE_1), (Position.AveragePrice + (TP_INICIAL_VENTAS * TickSize)) , @"TP_VENTA1", @"VENTA1");
				BE_TRIGGER = (Position.AveragePrice + (BE_DISTANCE_VENTA * TickSize)) ;
			}
			
			 // Ventas BE Activated
			if ((Position.MarketPosition == MarketPosition.Short)
				 && (Close[0] >= BE_TRIGGER))
			{
				BE_ON = true;
				ExitShortStopMarket(Convert.ToInt32(ORDER_SIZE_1), (Position.AveragePrice + (BE_OFFSET_VENTA * TickSize)) , @"BE_VENTA1", @"VENTA1");
				ExitLongLimit(Convert.ToInt32(ORDER_SIZE_1), (Position.AveragePrice + (TP_INICIAL_VENTAS * TickSize)) , @"TP_VENTA1", @"VENTA1");
			}


            // Ventas SALVAVIDAS
            if ((Position.MarketPosition == MarketPosition.Short)
               && Close[0] >= SL_INCIAL_VENTA && BE_ON == false)
            {
                ExitLong(@"COMPRA1");
            }
            if ((Position.MarketPosition == MarketPosition.Long)
                && Close[0] >= BE_OFFSET_VENTA && BE_ON == true)
            {
                ExitLong(@"COMPRA1");
            }

        }

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI_PERIOS", Order=1, GroupName="Parameters")]
		public int RSI_PERIOS
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI_UP", Order=2, GroupName="Parameters")]
		public int RSI_UP
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI_LOW", Order=3, GroupName="Parameters")]
		public int RSI_LOW
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="COMPRAS", Order=4, GroupName="Parameters")]
		public bool COMPRAS
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="VENTAS", Order=5, GroupName="Parameters")]
		public bool VENTAS
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ORDER_SIZE_1", Order=6, GroupName="Parameters")]
		public int ORDER_SIZE_1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BE_BOOL", Order=7, GroupName="Parameters")]
		public bool BE_BOOL
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SL_INICIAL", Order=8, GroupName="Parameters")]
		public int SL_INICIAL
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BE_DISTANCE", Order=9, GroupName="Parameters")]
		public int BE_DISTANCE
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BE_OFFSET", Order=10, GroupName="Parameters")]
		public int BE_OFFSET
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TP_INICIAL", Order=11, GroupName="Parameters")]
		public int TP_INICIAL
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SL_INCIAL_VENTA", Order=12, GroupName="Parameters")]
		public int SL_INCIAL_VENTA
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BE_DISTANCE_VENTA", Order=13, GroupName="Parameters")]
		public int BE_DISTANCE_VENTA
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TP_INICIAL_VENTAS", Order=14, GroupName="Parameters")]
		public int TP_INICIAL_VENTAS
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BE_OFFSET_VENTA", Order=15, GroupName="Parameters")]
		public int BE_OFFSET_VENTA
		{ get; set; }
		#endregion

	}
}
