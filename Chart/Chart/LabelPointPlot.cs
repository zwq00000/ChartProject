using System;
using System.Drawing;
using System.Data;

namespace xuzhenzhen.com.chart
{

	/// <summary>
	/// Encapsulates functionality
	/// </summary>
	public class LabelPointPlot : PointPlot, ISequencePlot
	{

		/// <summary>
		/// This class us used in conjunction with SequenceAdapter to interpret data
		/// specified to the TextPlot class.
		/// </summary>
		private class TextDataAdapter
		{

			private object data_;
			private object dataSource_;
			private string dataMember_;

			public TextDataAdapter( object dataSource, string dataMember, object data )
			{
				this.data_ = data;
				this.dataSource_ = dataSource;
				this.dataMember_ = dataMember;
			}

			public string this[int i]
			{
				get
				{

					// this is inefficient [could set up delegates in constructor].

					if (data_ is string[])
					{
						return ((string[])data_)[i];
					}
					
					if (data_ is string)
					{
						if (dataSource_ == null)
						{
							throw new NPlotException( "Error: DataSource null" );
						}

						System.Data.DataRowCollection rows;

						if ( dataSource_ is System.Data.DataSet )
						{
							if (dataMember_ != null)
							{
								// TODO error check
								rows = ((DataTable)((DataSet)dataSource_).Tables[dataMember_]).Rows;
							}
							else
							{
								// TODO error check
								rows = ((DataTable)((DataSet)dataSource_).Tables[0]).Rows;
							}
						}

						else if (dataSource_ is System.Data.DataTable )
						{
							rows = ((DataTable)dataSource_).Rows;
						}

						else
						{
							throw new NPlotException ( "not implemented yet" );
						}

						return (string)((System.Data.DataRow)(rows[i]))[(string)data_];
					}

					if (data_ is System.Collections.ArrayList)
					{
						object dataPoint = ((System.Collections.ArrayList)data_)[i];
						if (dataPoint is string)
							return (string)dataPoint;
						throw new NPlotException( "TextDataAdapter: data not in recognised format" );
					}
					
					if (data_ == null)
					{
						return "text";
					}

					throw new NPlotException( "Text data not of recognised type" );
				}
			}


			public int Count
			{
				get
				{
					// this is inefficient [could set up delegates in constructor].

					if (data_ == null)
					{
						return 0;
					}
					if (data_ is string[])
					{
						return ((string[])data_).Length;
					}
					if (data_ is System.Collections.ArrayList)
					{
						return ((System.Collections.ArrayList)data_).Count;
					}
					throw new NPlotException( "Text data not in correct format" );
				}
			}

		}


		/// <summary>
		/// Enumeration of all label positions relative to a point.
		/// </summary>
		public enum LabelPositions
		{
			/// <summary>
			/// Above the point
			/// </summary>
			Above,
			/// <summary>
			/// Below the point
			/// </summary>
			Below,
			/// <summary>
			/// To the left of the point
			/// </summary>
			Left,
			/// <summary>
			/// To the right of the point
			/// </summary>
			Right
		}


		/// <summary>
		/// Default Constructor
		/// </summary>
		public LabelPointPlot()
		{
		}


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="marker">The marker type to use for this plot.</param>
		public LabelPointPlot( Marker marker ) 
			: base( marker )
		{
		}

		
		/// <summary>
		/// The position of the text label in relation to the point.
		/// </summary>
		public LabelPositions LabelTextPosition
		{
			get
			{
				return labelTextPosition_;
			}
			set
			{
				labelTextPosition_ = value;
			}
		}
		private LabelPositions labelTextPosition_ = LabelPositions.Above;


		/// <summary>
		/// The text datasource to attach to each point.
		/// </summary>
		public object TextData
		{
			get
			{
				return textData_;
			}
			set
			{
				textData_ = value;
			}
		}
		object textData_;


		/// <summary>
		/// The Font used to write text.
		/// </summary>
		public Font Font
		{
			get
			{
				return font_;
			}
			set
			{
				font_ = value;
			}
		}
		private Font font_ = new Font( "Arial", 8.0f,FontStyle.Regular );


		/// <summary>
		/// Draws the plot on a GDI+ surface against the provided x and y axes.
		/// </summary>
		/// <param name="g">The GDI+ surface on which to draw.</param>
		/// <param name="xAxis">The X-Axis to draw against.</param>
		/// <param name="yAxis">The Y-Axis to draw against.</param>
		public override void Draw( Graphics g, PhysicalAxis xAxis, PhysicalAxis yAxis )
		{
			SequenceAdapter data = 
				new SequenceAdapter( this.DataSource, this.DataMember, this.OrdinateData, this.AbscissaData );

			TextDataAdapter textData =
				new TextDataAdapter( this.DataSource, this.DataMember, this.TextData );

			// first plot the marker
		    // we can do this cast, since the constructor accepts only this type!
			for (int i=0; i<data.Count; ++i)
			{
				try
				{
					PointD pt = data[i];
					if ( !Double.IsNaN(pt.X) && !Double.IsNaN(pt.Y) )
					{
						PointF xPos = xAxis.WorldToPhysical( pt.X, false);
						PointF yPos = yAxis.WorldToPhysical( pt.Y, false);
						Marker.Draw( g, (int)xPos.X, (int)yPos.Y );
						if ( textData[i] != "" )
						{
							SizeF size = g.MeasureString( textData[i], this.Font );
                            Brush brush = new SolidBrush(Color.Black);
							switch (labelTextPosition_)
							{
								case LabelPositions.Above:
									g.DrawString( textData[i], font_, brush, xPos.X-size.Width/2,yPos.Y-size.Height-Marker.Size*2/3);
									break;
								case LabelPositions.Below:
                                    g.DrawString(textData[i], font_, brush, xPos.X - size.Width / 2, yPos.Y + Marker.Size * 2 / 3);
									break;
								case LabelPositions.Left:
                                    g.DrawString(textData[i], font_, brush, xPos.X - size.Width - Marker.Size * 2 / 3, yPos.Y - size.Height / 2);
									break;
								case LabelPositions.Right:
                                    g.DrawString(textData[i], font_, brush, xPos.X + Marker.Size * 2 / 3, yPos.Y - size.Height / 2);
									break;
							}
						}
					}
				}
				catch
				{
					throw new NPlotException("Error in TextPlot.Draw");
				}
			}

		}


	}
}
