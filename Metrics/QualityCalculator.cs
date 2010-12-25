using System;

namespace Metrics
{
	/// <summary>
	/// Description of QualityCalculator.
	/// </summary>
	public static class QualityCalculator {
		#region Public Methods
		public static QualityCoefficient calculateSearchAffinity(String searchString) {
			return new QualityCoefficient();
		}
		public static QualityCoefficient calculatePeerQuality(Int32 buffered, Int32 queueSize) {
			return new QualityCoefficient();
		}
		public static QualityCoefficient calculateFileQuality() {
			return new QualityCoefficient();
		}
		#endregion
		public static void Main() {
			Console.WriteLine(QualityCalculator.calculateFileQuality());		
			Console.ReadLine();
		}
	}
}
