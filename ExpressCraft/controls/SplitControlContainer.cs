﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge.Html5;

namespace ExpressCraft
{
	public class SplitControlContainer : Control
	{
		public Control Panel1;
		public Control Panel2;
		public Control Splitter;
		private bool _isMouseDown = false;
		private Vector2 _mouseDownVector;
		private Vector2 _currentMouseDownVector;
		private int _startingSplitterPos;		
		private int _splitterPosition = -1;		

		private FixedSplitterPosition fixedSplitterPostion = FixedSplitterPosition.Panel1;

		public FixedSplitterPosition FixedSplitterPostion
		{
			get { return fixedSplitterPostion; }
			set {
				fixedSplitterPostion = value;
				RenderControls();
			}
		}

		public bool SplitterResizable = true;

		public int SplitterPosition
		{
			get { return _splitterPosition; }
			set {
				if(value < 0)
					value = 0;				
				_splitterPosition = value;
				RenderControls();
			}
		}
		
		private bool horizontal;

		public bool Horizontal
		{
			get { return horizontal; }
			set {
				if(value != horizontal)
				{
					RenderControls();
					horizontal = value;
				}				
			}
		}

		public override void Render()
		{
			base.Render();
			
			RenderControls();
		}
		private ClientRect _prevClientRect = null;		

		private void ResizeChildren()
		{
			if(this.LinkedForm != null)
			{
				if(Panel1 != null && Panel1.Content != null)
				{
					this.LinkedForm.Resizing(Panel1.Content);
				}
				if(Panel2 != null && Panel2.Content != null)
				{
					this.LinkedForm.Resizing(Panel2.Content);
				}
			}
		}

		public SplitControlContainer() : base("splitcontrol")
		{
			Panel1 = new Control();
			Panel2 = new Control();
			Splitter = new Control();

			Splitter.Content.OnMouseDown = (ev) =>
			{
				if(!SplitterResizable)
					return;
				_isMouseDown = true;
				_mouseDownVector = Helper.GetClientMouseLocation(ev);
				var maxSize = GetMaxSplitterSize();
				_startingSplitterPos = _splitterPosition > maxSize ? maxSize : _splitterPosition;

				ev.StopPropagation();
			};								

			OnResize = (ev) =>
			{
				if(this.LinkedForm != null)
				{
					if(!this.LinkedForm.IsVisible())
					{
						return;
					}
				}
				var clientRec = this.Content.GetBoundingClientRect();

				if(_prevClientRect == null)
				{
					_prevClientRect = clientRec;
				}

				if(fixedSplitterPostion != FixedSplitterPosition.Panel1)
				{
					double V1 = 0;
					double V2 = 0;
					bool dirty = false;

					if(Horizontal)
					{
						if(clientRec.Height != _prevClientRect.Height)
						{
							V1 = clientRec.Height;
							V2 = _prevClientRect.Height;
							dirty = true;
						}
					}
					else
					{
						if(clientRec.Width != _prevClientRect.Width)
						{
							V1 = clientRec.Width;
							V2 = _prevClientRect.Width;
							dirty = true;
						}
					}
					if(dirty)
					{
						switch(fixedSplitterPostion)
						{														
							case FixedSplitterPosition.Panel2:
								SplitterPosition = ((int)V1 - ((int)V2 - SplitterPosition));
								break;
							case FixedSplitterPosition.None:
								SplitterPosition = V1 == 0 || V2 == 0 ? 0 : (int)(SplitterPosition * (V1 / V2));
								break;
							default:
								break;
						}
					}
				}

				_prevClientRect = clientRec;
				
				RenderControls();

				ResizeChildren();
			};

			Content.OnMouseMove = (ev) =>
			{
				if(_isMouseDown)
				{
					_currentMouseDownVector = Helper.GetClientMouseLocation(ev);
					int x;
					if(horizontal)
					{
						x = _startingSplitterPos - (_mouseDownVector.Yi - _currentMouseDownVector.Yi);
					}
					else
					{
						x = _startingSplitterPos - (_mouseDownVector.Xi - _currentMouseDownVector.Xi);
					}
					var y = GetMaxSplitterSize();
					if(x > y)
					{
						x = y;
					}
					SplitterPosition = x;
					_currentMouseDownVector = _mouseDownVector;

					ResizeChildren();
				}
			};

			Content.OnMouseUp = (ev) =>
			{
				_isMouseDown = false;

				RenderControls();
			};

			this.AppendChildren(Panel1, Splitter, Panel2);
		}		

		private int GetMaxSplitterSize()
		{
			var maxSize = (int)(Horizontal ? this.Content.GetBoundingClientRect().Height : this.Content.GetBoundingClientRect().Width) - 12;
			if(maxSize < 0)
				maxSize = 0;
			return maxSize;
		}

		private void RenderControls()
		{
			var sp = SplitterPosition;
			if(_prevClientRect != null)
			{
				var maxSize = GetMaxSplitterSize();
				if(sp > maxSize)
				{
					sp = maxSize;
				}
			}

			if(Horizontal)
			{
				Panel1.ExchangeClass("splitvertical", "splithorizontal");
				Panel2.ExchangeClass("splitvertical", "splithorizontal");
				Splitter.ExchangeClass("splitvertical", "splitvertical");

				Panel1.Location = new Vector2(0, 0);
				Panel1.Width = "";
				Panel1.Height = sp;

				Splitter.Location = new Vector2(0, sp);
				Splitter.Width = "";

				Panel2.Location = new Vector2(0, sp + 12);
				Panel2.Width = "";
				Panel2.Height = "calc(100% - " + (sp + 12) + "px)"; ;
			}
			else
			{
				Panel1.ExchangeClass("splithorizontal", "splitvertical");
				Panel2.ExchangeClass("splithorizontal", "splitvertical");
				Splitter.ExchangeClass("splitterhorizontal", "splittervertical");

				Panel1.Location = new Vector2(0, 0);
				Panel1.Width = sp;
				Panel1.Height = "";

				Splitter.Location = new Vector2(sp, 0);
				Splitter.Height = "";

				Panel2.Location = new Vector2(sp + 12, 0);
				Panel2.Width = "calc(100% - " + (sp + 12) + "px)";
				Panel2.Height = "";
			}
		}
	}
}
