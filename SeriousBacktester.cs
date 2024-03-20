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
		private ATR atr;
		private double bbw;
		
		private double beTriggerPrice;
		private bool BreakEvenOn;
		
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
//				BBWLength			= 30;
//				BBWStdv				= 2;
//				BBWHourTrigger		= 0.2;
				RsiPeriods			= 13;
				RsiUpper			= 70;
				RsiLower			= 30;
				BBLength			= 24;
				BBStdv				= 2;
				ATRLength			= 14; //20 Funciona bien en 15m
				BBWTrigger			= 0.8;
				StopLossPerATR		= 1.6;
				TakeProfitPerATR	= 1.6;
				bbwSwitch 			= false;
				Ventas 				= false;
				Compras				= true;
				BESwitch 			= false;
				BETriggerATR 		= 1.6;
				BEOffsetATR			= 0.6;
                Inicio				= DateTime.Parse("14:00", System.Globalization.CultureInfo.InvariantCulture);
                Fin 				= DateTime.Parse("18:30", System.Globalization.CultureInfo.InvariantCulture);
			
			
			}			
			else if (State == State.Configure)
			{
			}else if (State == State.DataLoaded)
			{	
				bb = Bollinger(BBStdv, BBLength);
				bb.Plots[0].Brush = Brushes.Snow;
				bb.Plots[1].Brush = Brushes.Snow;
				bb.Plots[2].Brush = Brushes.Snow;					
				AddChartIndicator(bb);
				
				rsi = RSI(Close, Convert.ToInt32(RsiPeriods), 3);
				rsi.Plots[0].Brush = Brushes.Snow;
				rsi.Plots[1].Brush = Brushes.Transparent;						
				AddChartIndicator(rsi);
				
				atr = ATR(ATRLength);
				atr.Plots[0].Brush = Brushes.Snow;					
				AddChartIndicator(atr);
				
				
			}
		}

		protected override void OnBarUpdate()
		{
			
			if(CurrentBar < 10) return;
			
			//Indicador BBW
			double midValue = bb.Middle[0];
			double upperValue = bb.Upper[0];
			double lowerValue = bb.Lower[0];
			bbw = Math.Round((upperValue - lowerValue) * 0.1, 1);
			Print("Ancho de las bandas: " + bbw);
			Draw.TextFixed(this, "Ancho de las bandas", "BBW: "+bbw.ToString(), TextPosition.TopRight);
			
			//Mostrando ATR
			Draw.TextFixed(this, "ATR", "ATR: "+atr[0].ToString(), TextPosition.BottomRight);

			//Logica
			if(Position.MarketPosition == MarketPosition.Flat){
				BreakEvenOn = false;
				// Si no hay ordenes apagamos el BE
			}
			
			if(Compras){
				
				//Compras CON BBW
				if(Position.MarketPosition == MarketPosition.Flat && bbwSwitch){
					if(VelaAlcista(0) && VelaEnvuelveHigh(0,1) && VelaBajista(1) && Low[1] < bb.Lower[1] && rsi[1] < RsiLower && bbw  < BBWTrigger){ //CAMBIAR DESPUES DE IGUAL A MAYOR
						EnterLong(DefaultQuantity, @"COMPRA");
					}
				}
				
				//Compras SIN BBW
				if(Position.MarketPosition == MarketPosition.Flat && bbwSwitch == false){
					if(VelaAlcista(0) && VelaEnvuelveHigh(0,1) && VelaBajista(1) && Low[1] < bb.Lower[1] && rsi[1] < RsiLower){
						EnterLong(DefaultQuantity, @"COMPRA");
					}
				}
			}
			
			
			
			if(Ventas){
				//Ventas CON BBW
				if(Position.MarketPosition == MarketPosition.Flat && bbwSwitch){
					if(VelaBajista(0) && VelaEnvuelveLow(0,1) && VelaAlcista(1) && High[1] > bb.Upper[1] && rsi[1] > RsiUpper && bbw < BBWTrigger){ //CAMBIAR DESPUES DE IGUAL A MAYOR
						EnterShort(DefaultQuantity, @"VENTA");
					}
				}
				
				//Ventas SIN BBW
				if(Position.MarketPosition == MarketPosition.Flat && bbwSwitch == false){
					if(VelaBajista(0) && VelaEnvuelveLow(0,1) && VelaAlcista(1) && High[1] > bb.Upper[1] && rsi[1] > RsiUpper){
						EnterShort(DefaultQuantity, @"VENTA");
					}
				}
			}
			
			
			//Order Management
			//Si hay ordenes Abiertas
			if(Position.MarketPosition != MarketPosition.Flat){

				//SL y TG Compras
				if(Position.MarketPosition == MarketPosition.Long && BreakEvenOn == false){
					
					//Calculamos la distancia
					double slDistance = atr[1] * StopLossPerATR;
					double tpDistance = atr[1] * TakeProfitPerATR;
					
					//Calculamos los targets
					double slPrice = Position.AveragePrice - slDistance;
					double tpPrice = Position.AveragePrice + tpDistance;
					
					//Colocamos los targets
					ExitLongStopMarket(DefaultQuantity, slPrice, @"SL COMPRA", @"COMPRA");
					ExitLongLimit(DefaultQuantity, tpPrice, @"TP COMPRA", @"COMPRA");
					
					// Seteamos el BE Offset
					BreakEvenOn = false;
					double beTriggerDistance = atr[1] * BETriggerATR;
					beTriggerPrice = Position.AveragePrice + beTriggerDistance; 
					// Seteamos el precio trigger para esta orden
					
				}
				
				// Activando BE Compras
				if(Position.MarketPosition == MarketPosition.Long && Close[0] > beTriggerPrice && BESwitch == true){ // Cruza por encima el precio Trigger
					
					double beOffsetDistance = atr[1] * BEOffsetATR;
					double beOffsetPrice = Position.AveragePrice + beOffsetDistance;
					
					double tpDistance = atr[1] * TakeProfitPerATR;
					double tpPrice = Position.AveragePrice + tpDistance;
					
					ExitLongStopMarket(Position.AveragePrice + beOffsetPrice, @"BE COMPRA", @"COMPRA");
					ExitLongLimit(DefaultQuantity, tpPrice, @"TP COMPRA", @"COMPRA");
					
					//Dibujamos los targets
					Draw.Dot(this, "SL ATR LONG", true, 1, beOffsetPrice, Brushes.Red);
					Draw.Dot(this, "TP ATR LONG", true, 1, tpPrice, Brushes.DodgerBlue);
					
					BreakEvenOn = true;
					
				}
				
				
				//SL y TG Ventas
				if(Position.MarketPosition == MarketPosition.Short && BreakEvenOn == false){
					
					//Calculamos la distancia
					double slDistance = atr[1] * StopLossPerATR;
					double tpDistance = atr[1] * TakeProfitPerATR;
					
					//Calculamos los targets
					double slPrice = Position.AveragePrice + slDistance;
					double tpPrice = Position.AveragePrice - tpDistance;
					
					//Colocamos los targets
					ExitShortStopMarket(DefaultQuantity, slPrice, @"SL VENTA", @"VENTA");
					ExitShortLimit(DefaultQuantity, tpPrice, @"TP VENTA", @"VENTA");
						
					//Dibujamos los targets
					Draw.Dot(this, "SL ATR LONG", true, 1, slPrice, Brushes.Red);
					Draw.Dot(this, "TP ATR LONG", true, 1, tpPrice, Brushes.DodgerBlue);
					
					// Seteamos el BE Offset
					BreakEvenOn = false;
					double beTriggerDistance = atr[1] * BETriggerATR;
					beTriggerPrice = Position.AveragePrice - beTriggerDistance; 
					// Seteamos el precio trigger para esta orden
					
				}
				
				
				// Activando BE Ventas
				if(Position.MarketPosition == MarketPosition.Short && Close[0] < beTriggerPrice && BESwitch == true){ // Cruza por debajo el precio Trigger
										
					double beOffsetDistance = atr[1] * BEOffsetATR;
					double beOffsetPrice = Position.AveragePrice - beOffsetDistance;
					
					double tpDistance = atr[1] * TakeProfitPerATR;
					double tpPrice = Position.AveragePrice - tpDistance;
					
					ExitShortStopMarket(Position.AveragePrice - beOffsetPrice, @"BE VENTA", @"VENTA");
					ExitShortLimit(DefaultQuantity, tpPrice, @"TP VENTA", @"VENTA");
					BreakEvenOn = true;
					
				}
				
					
			}
			
			
			
			
			
		}
		
		private bool VelaBajista(int indice){
			if (Close[indice] < Open[indice]){return true;}
			else{return false;}
		}
		
		private bool VelaAlcista(int indice){
			if (Close[indice] > Open[indice]){return true;}
			else{return false;}
		}
		
		private bool VelaEnvuelveHigh(int indiceActual, int indicePrevio){
			if (Close[indiceActual] > High[indicePrevio]){return true;}
			else{return false;}
		}
		
		private bool VelaEnvuelveLow(int indiceActual, int indicePrevio){
			if (Close[indiceActual] < Low[indicePrevio]){return true;}
			else{return false;}
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
		[Range(0.1, double.MaxValue)]
		[Display(Name="BBStdv", Order=5, GroupName="Parameters")]
		public int BBStdv
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ATRLength", Order=8, GroupName="Parameters")]
		public int ATRLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="BBWTrigger", Order=9, GroupName="Parameters")]
		public double BBWTrigger
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="BE Trigger ATR", Order=11, GroupName="Parameters")]
		public double BETriggerATR
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="BE Offset ATR", Order=11, GroupName="Parameters")]
		public double BEOffsetATR
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
		
        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Inicio", Description = "Hora de inicio", Order = 19, GroupName = "Parameters")]
        public DateTime Inicio
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Fin", Description = "Hora Final", Order = 20, GroupName = "Parameters")]
        public DateTime Fin
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "BBW Switch", Description = "BBW Switch", Order = 0, GroupName = "Parameters")]
		public bool bbwSwitch
		{ get; set; }

        [NinjaScriptProperty]
        [Display(Name = "BE Switch", Description = "BES witch", Order = 0, GroupName = "Parameters")]
		public bool BESwitch
		{ get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Compras", Description = "Compras", Order = 0, GroupName = "Parameters")]
		public bool Compras
		{ get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Ventas", Description = "Ventas", Order = 0, GroupName = "Parameters")]
		public bool Ventas
		{ get; set; }
		
//		[NinjaScriptProperty]
//		[Range(1, int.MaxValue)]
//		[Display(Name="BBWLength", Order=6, GroupName="Parameters")]
//		public int BBWLength
//		{ get; set; }

//		[NinjaScriptProperty]
//		[Range(1, int.MaxValue)]
//		[Display(Name="BBWStdv", Order=7, GroupName="Parameters")]
//		public int BBWStdv
//		{ get; set; }

//		[NinjaScriptProperty]
//		[Display(Name="BBWHourTrigger", Order=10, GroupName="Parameters")]
//		public double BBWHourTrigger
//		{ get; set; }
		#endregion

	}
}
