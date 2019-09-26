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

        private static string cookiesVeryLow => "Cookies sell for record high prices as a cookie drought hits the market!";
        private static string cookiesQuiteLow => "Cookie prices rise as cookies become more and more scarce.";
        private static string cookiesQuiteHigh => "The proliferation of cookies cause prices to dip.";
        private static string cookiesVeryHigh => "Cookie prices drastically drop as cookies flood into the market.";

        private static string croissantsVeryLow => "Connoiseurs pay top-dollar for croissants as they become almost impossible to find!";
        private static string croissantsQuiteLow => "Pastry afficionados pay premium rates for croissants as croissant shortage hits market.";
        private static string croissantsQuiteHigh => "Croissant prices drop as they become more accessible and readily available.";
        private static string croissantsVeryHigh => "The market becomes saturated with croissants, causing prices to plummet.";

        private static string cakesVeryLow => "Cakes classified as rare luxury items, selling for exorbitant prices.";
        private static string cakesQuiteLow => "Customers wait in line and pay top-dollar to taste cakes as shortage hits the market.";
        private static string cakesQuiteHigh => "Cakes become commonplace. Prices dip slightly as customers no longer willing to pay top-dollar for cakes.";
        private static string cakesVeryHigh => "The immense amount of cakes in the market cause prices to drop when they are deemed 'plebian food'.";
    }
}
