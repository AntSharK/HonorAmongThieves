using System.Text;

namespace HonorAmongThieves.Cakery.GameLogic
{
    public static class TextGenerator
    {
        public static string GetNewsReport(CakeryRoom.MarketReport marketReport)
        {
            (var cookiesSold, var croissantsSold, var cakesSold) = marketReport.TotalSales;
            (var expectedCookies, var expectedCroissants, var expectedCakes) = CakeryRoom.ComputeExpectedSales(marketReport.CashInPreviousRound);

            var cookiePercentageSold = cookiesSold / (expectedCookies != 0 ? expectedCookies : 0.1);
            var croissantPercentageSold = croissantsSold / (expectedCroissants != 0 ? expectedCroissants : 0.1);
            var cakePercentageSold = cakesSold / (expectedCakes != 0 ? expectedCakes : 0.1);

            StringBuilder newsReport = new StringBuilder();
            if (cookiePercentageSold < 0.3 && marketReport.TotalSales.cookiesSold > 0)
            {
                newsReport.AppendLine(cookiesVeryLow);
            }
            else if (cookiePercentageSold < 0.6 && marketReport.TotalSales.cookiesSold > 0)
            {
                newsReport.AppendLine(cookiesQuiteLow);
            }
            else if (cookiePercentageSold > 1.3 && marketReport.TotalSales.cookiesSold > 0)
            {
                newsReport.Append(cookiesQuiteHigh);
            }
            else if (cookiePercentageSold > 1.8 && marketReport.TotalSales.cookiesSold > 0)
            {
                newsReport.Append(cookiesVeryHigh);
            }

            if (croissantPercentageSold < 0.3 && marketReport.TotalSales.croissantsSold > 0)
            {
                newsReport.AppendLine(croissantsVeryLow);
            }
            else if (croissantPercentageSold < 0.6 && marketReport.TotalSales.croissantsSold > 0)
            {
                newsReport.AppendLine(croissantsQuiteLow);
            }
            else if (croissantPercentageSold > 1.3 && marketReport.TotalSales.croissantsSold > 0)
            {
                newsReport.Append(croissantsQuiteHigh);
            }
            else if (croissantPercentageSold > 1.8 && marketReport.TotalSales.croissantsSold > 0)
            {
                newsReport.Append(croissantsVeryHigh);
            }

            if (cakePercentageSold < 0.3 && marketReport.TotalSales.cakesSold > 0)
            {
                newsReport.AppendLine(cakesVeryLow);
            }
            else if (cakePercentageSold < 0.6 && marketReport.TotalSales.cakesSold > 0)
            {
                newsReport.AppendLine(cakesQuiteLow);
            }
            else if (cakePercentageSold > 1.3 && marketReport.TotalSales.cakesSold > 0)
            {
                newsReport.Append(cakesQuiteHigh);
            }
            else if (cakePercentageSold > 1.8 && marketReport.TotalSales.cakesSold > 0)
            {
                newsReport.Append(cakesVeryHigh);
            }

            return newsReport.ToString();
        }

        private static string cookiesVeryLow => "CookieS in short supply! Cookies sell for record high prices!";
        private static string cookiesQuiteLow => "Cookie prices rice as cookies become more and more scarce.";
        private static string cookiesQuiteHigh => "The widespread proliferation of cookies cause prices to drop.";
        private static string cookiesVeryHigh => "Cookie prices drastically drop as cookies flood into the market.";

        private static string croissantsVeryLow => "Croissants in short supply! Croissants sell for record high prices!";
        private static string croissantsQuiteLow => "Croissant prices rice as croissants become more and more scarce.";
        private static string croissantsQuiteHigh => "The widespread proliferation of croissants cause prices to drop.";
        private static string croissantsVeryHigh => "Croissant prices drastically drop as croissants flood into the market.";

        private static string cakesVeryLow => "Cakes in short supply! Cakes sell for record high prices!";
        private static string cakesQuiteLow => "Cake prices rice as cakes become more and more scarce.";
        private static string cakesQuiteHigh => "The widespread proliferation of cakes cause prices to drop.";
        private static string cakesVeryHigh => "Cake prices drastically drop as cakes flood into the market.";
    }
}
