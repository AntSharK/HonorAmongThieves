"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/cakeryHub").build();
var bakingMenuState = "upgrading"; // Can be: 'baking', 'upgrading', 'buyingingredients', 'usingupgrades'

// -------------------------
// ----- GAME OBJECTS ------
// -------------------------

// The player state currently, not including this round's purchases
var playerState = {
    resources: {},
    upgrades: {},
    bakedGoods: {},
    justPurchasedUpgrades: {},
}

var gameState = {
    currentPrices: {},
    currentMarket: {}
}

document.getElementById("startbutton").addEventListener("click", function (event) {
    var gamelength = Number(document.getElementById("gamelength").value);
    var startingcash = Number(document.getElementById("startingcash").value);
    var annualallowance = Number(document.getElementById("annualallowance").value);
    var upgradeallowance = Number(document.getElementById("upgradeallowance").value);
    connection.invoke("StartRoom", roomId, gamelength, startingcash, annualallowance, upgradeallowance).catch(function (err) {
        return console.error(err.toString());
    });
    document.getElementById("startButtonDiv").style.display = "none";
    event.preventDefault();
});

// -------------------------
// ----- UTILITY METHODS ---
// -------------------------
function getNumber(elementId, roundDown) {
    var element = document.getElementById(elementId);
    if (element == null) {
        return;
    }

    var number = element.value;

    // Do bounds checking and reflect changes in UI
    if (number.length == 0 || number <= 0) {
        if (roundDown) {
            if (number == 0) {
                document.getElementById(elementId).value = "";
                document.getElementById(elementId).placeholder = "0";
            }
            else {
                document.getElementById(elementId).value = number;
            }
        }
        else if (number < 0) {
            document.getElementById(elementId).value = "";
            document.getElementById(elementId).placeholder = "0.0";
        }

        number = 0;
    }
    else if (roundDown && number % 1 != 0) {
        number = number - number % 1;
        document.getElementById(elementId).value = number;
    }

    if (number > element.max) {
        number = element.max;
        document.getElementById(elementId).value = number;
    }

    return number;
}

// -----------------------------
// ----- STATE: MENU -----------
// -----------------------------

// Update from server-side
connection.on("UpdateProductionState", function (currentPrices, currentMarket, playerResources, playerUpgrades, playerJustPurchasedUpgrades, playerBakedGoods) {
    gameState.currentPrices = currentPrices;
    gameState.currentMarket = currentMarket;
    playerState.resources = playerResources;
    playerState.upgrades = playerUpgrades;
    playerState.justPurchasedUpgrades = playerJustPurchasedUpgrades;
    playerState.bakedGoods = playerBakedGoods;

    showMenu();
});

// Display the common buttons and tables for baking menu
function showCommonMenuButtons() {
    document.getElementById("currentyear").textContent = "ROUND: " + (gameState.currentMarket.currentYear + 1) + "/" + gameState.currentMarket.maxYears;

    var elements = document.getElementsByClassName("commonmenubutton");
    for (var i = 0; i < elements.length; i++) {
        elements[i].style.display = "block";
    }

    document.getElementById("moneyowned2").textContent = "Cash Available: $" + (playerState.resources.money / 100).toFixed(2);
    document.getElementById("flourowned2").textContent = Math.ceil(playerState.resources.flour) + "g";
    document.getElementById("sugarowned2").textContent = Math.ceil(playerState.resources.sugar) + "g";
    document.getElementById("butterowned2").textContent = Math.ceil(playerState.resources.butter) + "g";

    document.getElementById("cookiesbaked2").textContent = playerState.bakedGoods.cookies;
    document.getElementById("croissantsbaked2").textContent = playerState.bakedGoods.croissants;
    document.getElementById("cakesbaked2").textContent = playerState.bakedGoods.cakes;
}

// Display the baking menu, looking at the baking menu state
function showMenu() {
    if (bakingMenuState == "buyingingredients") {
        showGroceries();
    }
    else if (bakingMenuState == "baking") {
        showBakeMenu();
    }
    else if (bakingMenuState == "upgrading") {
        showUpgradeMenu();
    }
    else if (bakingMenuState == "usingupgrades") {
        showUseUpgradeMenu();
    }

    showCommonMenuButtons();
}

// Switch to "Groceries" view
document.getElementById("switchtogroceryviewbutton").addEventListener("click", function (event) {
    bakingMenuState = "buyingingredients";
    showMenu();
    event.preventDefault();
});

// Switch to "Baking" view
document.getElementById("switchtobakeviewbutton").addEventListener("click", function (event) {
    bakingMenuState = "baking";
    showMenu();
    event.preventDefault();
});

// Click on "Upgrade" view
document.getElementById("switchtoupgradeviewbutton").addEventListener("click", function (event) {
    bakingMenuState = "upgrading";
    showMenu();
    event.preventDefault();
});

// Click on "Use Upgrades" view
document.getElementById("switchtouseupgradeviewbutton").addEventListener("click", function (event) {
    bakingMenuState = "usingupgrades";
    showMenu();
    event.preventDefault();
});

// ---------------------------------
// ----- STATE: MENU (GROCERIES) ---
// ---------------------------------
function showGroceries() {
    changeUiState("BUY INGREDIENTS", "buyingredients");

    // Reset the input forms
    document.getElementById("buyingredientsbutton").disabled = true;
    document.getElementById("buyingredientsbutton").value = "BUY INGREDIENTS";

    updateIngredientsCost();

    document.getElementById("flourprice").textContent = "$" + (gameState.currentPrices.flour / 100).toFixed(2);
    document.getElementById("butterprice").textContent = "$" + (gameState.currentPrices.butter / 100).toFixed(2);
    document.getElementById("sugarprice").textContent = "$" + (gameState.currentPrices.sugar / 100).toFixed(2);
}

document.getElementById("buyingredientsbutton").addEventListener("click", function (event) {
    var moneySpent = getIngredientsCost();

    var butterbought = document.getElementById("buybutteramount").value;
    if (butterbought.length == 0) {
        butterbought = 0;
    }
    var flourbought = document.getElementById("buyflouramount").value;
    if (flourbought.length == 0) {
        flourbought = 0;
    }
    var sugarbought = document.getElementById("buysugaramount").value;
    if (sugarbought.length == 0) {
        sugarbought = 0;
    }

    if (moneySpent > playerState.resources.money) {
        window.alert("NOT ENOUGH MONEY! Ingredients cost $" + (moneySpent / 100).toFixed(2)
            + " but you only have $" + (playerState.resources.money / 100).toFixed(2));
    }
    else {
        document.getElementById("buybutteramount").value = "";
        document.getElementById("buybutteramount").placeholder = "0.0";
        document.getElementById("buyflouramount").value = "";
        document.getElementById("buyflouramount").placeholder = "0.0";
        document.getElementById("buysugaramount").value = "";
        document.getElementById("buysugaramount").placeholder = "0.0";
        document.getElementById("ingredientcost").textContent = "";
        document.getElementById("buyingredientsbutton").disabled = true;

        connection.invoke("BuyIngredients", roomId, userName, butterbought, flourbought, sugarbought).catch(function (err) {
            return console.error(err.toString());
        });
    }

    event.preventDefault();
});

function getIngredientsCost() {
    var butterbought = getNumber("buybutteramount", false /*Don't round down*/);
    var flourbought = getNumber("buyflouramount", false /*Don't round down*/);
    var sugarbought = getNumber("buysugaramount", false /*Don't round down*/);

    // Client-side check to make sure this is possible
    var moneySpent = gameState.currentPrices.butter * butterbought
        + gameState.currentPrices.flour * flourbought
        + gameState.currentPrices.sugar * sugarbought;

    return moneySpent;
}

document.getElementById("buyflouramount").addEventListener("input", function (event) {
    updateIngredientsCost();
});
document.getElementById("buysugaramount").addEventListener("input", function (event) {
    updateIngredientsCost();
});
document.getElementById("buybutteramount").addEventListener("input", function (event) {
    updateIngredientsCost();
});

function updateIngredientsCost() {
    var ingredientsCost = getIngredientsCost();

    // Early abort if nothing is being bought
    if (ingredientsCost <= 0) {
        document.getElementById("ingredientcost").textContent = "";
        document.getElementById("buyingredientsbutton").disabled = true;
        document.getElementById("buyingredientsbutton").value = "BUY INGREDIENTS";
        return;
    }

    document.getElementById("ingredientcost").textContent = "$" + (ingredientsCost / 100).toFixed(2) + "/$" + (playerState.resources.money / 100).toFixed(2);

    if (ingredientsCost > playerState.resources.money) {
        document.getElementById("ingredientcost").style.color = "red";
        document.getElementById("buyingredientsbutton").disabled = true;
        document.getElementById("buyingredientsbutton").value = "NOT ENOUGH MONEY";
    }
    else {
        document.getElementById("ingredientcost").style.color = "blue";
        document.getElementById("buyingredientsbutton").disabled = false;
        document.getElementById("buyingredientsbutton").value = "BUY INGREDIENTS";
    }
}

// ----------------------------
// ----- STATE: MENU (BAKE) ---
// ----------------------------
function showBakeMenu() {
    changeUiState("BAKE GOODS", "bakegoods");

    // Reset the input forms
    document.getElementById("bakethingsbutton").disabled = true;
    document.getElementById("bakethingsbutton").value = "BAKE THINGS";
    updateBakingCost();

    // Update the available resources and current prices
    var cakeCost = playerState.bakedGoods.cakeCost;
    var croissantCost = playerState.bakedGoods.croissantCost;
    var cookieCost = playerState.bakedGoods.cookieCost;
    document.getElementById("cookiepricebutter").textContent = Math.ceil(cookieCost.item1) + "g";
    document.getElementById("cookiepriceflour").textContent = Math.ceil(cookieCost.item2) + "g";
    document.getElementById("cookiepricesugar").textContent = Math.ceil(cookieCost.item3) + "g";
    document.getElementById("croissantpricebutter").textContent = Math.ceil(croissantCost.item1) + "g";
    document.getElementById("croissantpriceflour").textContent = Math.ceil(croissantCost.item2) + "g";
    document.getElementById("croissantpricesugar").textContent = Math.ceil(croissantCost.item3) + "g";
    document.getElementById("cakepricebutter").textContent = Math.ceil(cakeCost.item1) + "g";
    document.getElementById("cakepriceflour").textContent = Math.ceil(cakeCost.item2) + "g";
    document.getElementById("cakepricesugar").textContent = Math.ceil(cakeCost.item3) + "g";

    document.getElementById("cookierevenue").textContent = "$" + (gameState.currentPrices.cookies / 100).toFixed(2);
    document.getElementById("croissantrevenue").textContent = "$" + (gameState.currentPrices.croissants / 100).toFixed(2);
    document.getElementById("cakerevenue").textContent = "$" + (gameState.currentPrices.cakes / 100).toFixed(2);
}

// Click on baking things
document.getElementById("bakethingsbutton").addEventListener("click", function (event) {
    var cookiesBaked = document.getElementById("bakecookiesamount").value;
    if (cookiesBaked.length == 0) {
        cookiesBaked = 0;
    }
    var croissantsBaked = document.getElementById("bakecroissantsamount").value;
    if (croissantsBaked.length == 0) {
        croissantsBaked = 0;
    }
    var cakesBaked = document.getElementById("bakecakesamount").value;
    if (cakesBaked.length == 0) {
        cakesBaked = 0;
    }

    var cakeCost = playerState.bakedGoods.cakeCost;
    var croissantCost = playerState.bakedGoods.croissantCost;
    var cookieCost = playerState.bakedGoods.cookieCost;

    // Client-side check to make sure this is possible
    var butterUsed = cookieCost.item1 * cookiesBaked + croissantCost.item1 * croissantsBaked + cakeCost.item1 * cakesBaked;
    var flourUsed = cookieCost.item2 * cookiesBaked + croissantCost.item2 * croissantsBaked + cakeCost.item2 * cakesBaked;
    var sugarUsed = cookieCost.item3 * cookiesBaked + croissantCost.item3 * croissantsBaked + cakeCost.item3 * cakesBaked;
    var moneyUsed = cookieCost.item4 * cookiesBaked + croissantCost.item4 * croissantsBaked + cakeCost.item4 * cakesBaked;

    if (moneyUsed > playerState.resources.money
        || flourUsed > playerState.resources.flour
        || butterUsed > playerState.resources.butter
        || sugarUsed > playerState.resources.sugar) {
        window.alert("NOT ENOUGH RESOURCES! Ingredients cost $" + (moneyUsed / 100).toFixed(2)
            + ", " + butterUsed + "g butter, " + flourUsed + "g flour, " + sugarUsed + "g sugar.");
    }
    else {
        document.getElementById("bakecookiesamount").value = "";
        document.getElementById("bakecookiesamount").placeholder = 0;
        document.getElementById("bakecroissantsamount").value = "";
        document.getElementById("bakecroissantsamount").placeholder = 0;
        document.getElementById("bakecakesamount").value = "";
        document.getElementById("bakecakesamount").placeholder = 0;
        document.getElementById("flourforbaking").textContent = "";
        document.getElementById("sugarforbaking").textContent = "";
        document.getElementById("butterforbaking").textContent = "";

        connection.invoke("BakeGoods", roomId, userName, cookiesBaked, croissantsBaked, cakesBaked).catch(function (err) {
            return console.error(err.toString());
        });
    }

    event.preventDefault();
});

document.getElementById("bakecookiesamount").addEventListener("input", function (event) {
    updateBakingCost();
});
document.getElementById("bakecroissantsamount").addEventListener("input", function (event) {
    updateBakingCost();
});
document.getElementById("bakecakesamount").addEventListener("input", function (event) {
    updateBakingCost();
});

function updateBakingCost() {
    // Portions of this are copy+pasted from a function that exists when clicking the "Bake" button
    var cookiesBaked = getNumber("bakecookiesamount", true);
    var croissantsBaked = getNumber("bakecroissantsamount", true);
    var cakesBaked = getNumber("bakecakesamount", true);

    // Validate input and change UI
    if (cookiesBaked.length == 0 || cookiesBaked < 0) {
        cookiesBaked = 0;
        document.getElementById("bakecookiesamount").value = cookiesBaked;
    }
    else if (cookiesBaked % 1 != 0) {
        cookiesBaked = cookiesBaked - cookiesBaked % 1;
        document.getElementById("bakecookiesamount").value = cookiesBaked;
    }

    if (croissantsBaked.length == 0 || croissantsBaked < 0) {
        croissantsBaked = 0;
        document.getElementById("bakecroissantsamount").value = croissantsBaked;
    }
    else if (croissantsBaked % 1 != 0) {
        croissantsBaked = croissantsBaked - croissantsBaked % 1;
        document.getElementById("bakecroissantsamount").value = croissantsBaked;
    }

    if (cakesBaked.length == 0 || cakesBaked < 0) {
        cakesBaked = 0;
        document.getElementById("bakecakesamount").value = cakesBaked;
    }
    else if (cakesBaked % 1 != 0) {
        cakesBaked = cakesBaked - cakesBaked % 1;
        document.getElementById("bakecakesamount").value = cakesBaked;
    }

    // Early abort if nothing is being baked
    if (cookiesBaked + croissantsBaked + cakesBaked <= 0) {
        document.getElementById("bakethingsbutton").disabled = true;
        document.getElementById("bakethingsbutton").value = "BAKE THINGS";
        document.getElementById("flourforbaking").textContent = "";
        document.getElementById("sugarforbaking").textContent = "";
        document.getElementById("butterforbaking").textContent = "";
        document.getElementById("bakingingredientshortage").style.display = "none";
        return;
    }

    var cakeCost = playerState.bakedGoods.cakeCost;
    var croissantCost = playerState.bakedGoods.croissantCost;
    var cookieCost = playerState.bakedGoods.cookieCost;

    var butterUsed = cookieCost.item1 * cookiesBaked + croissantCost.item1 * croissantsBaked + cakeCost.item1 * cakesBaked;
    var flourUsed = cookieCost.item2 * cookiesBaked + croissantCost.item2 * croissantsBaked + cakeCost.item2 * cakesBaked;
    var sugarUsed = cookieCost.item3 * cookiesBaked + croissantCost.item3 * croissantsBaked + cakeCost.item3 * cakesBaked;
    var moneyUsed = cookieCost.item4 * cookiesBaked + croissantCost.item4 * croissantsBaked + cakeCost.item4 * cakesBaked;
    // Copy+paste ends here

    document.getElementById("butterforbaking").textContent = Math.ceil(butterUsed) + "g";
    document.getElementById("flourforbaking").textContent = Math.ceil(flourUsed) + "g";
    document.getElementById("sugarforbaking").textContent = Math.ceil(sugarUsed) + "g";

    var bakingEnabled = true;

    if (butterUsed > playerState.resources.butter) {
        document.getElementById("butterforbaking").style.color = "red";
        bakingEnabled = false;
    }
    else {
        document.getElementById("butterforbaking").style.color = "blue";
    }

    if (flourUsed > playerState.resources.flour) {
        document.getElementById("flourforbaking").style.color = "red";
        bakingEnabled = false;
    }
    else {
        document.getElementById("flourforbaking").style.color = "blue";
    }

    if (sugarUsed > playerState.resources.sugar) {
        document.getElementById("sugarforbaking").style.color = "red";
        bakingEnabled = false;
    }
    else {
        document.getElementById("sugarforbaking").style.color = "blue";
    }

    if (bakingEnabled) {
        document.getElementById("bakethingsbutton").value = "BAKE THINGS";
        document.getElementById("bakethingsbutton").disabled = false;
        document.getElementById("bakingingredientshortage").style.display = "none";
    }
    else {
        document.getElementById("bakethingsbutton").value = "LACK INGREDIENTS";
        document.getElementById("bakethingsbutton").disabled = true;
        document.getElementById("bakingingredientshortage").style.display = "";

        if (butterUsed > playerState.resources.butter) {
            document.getElementById("bakingbuttershortage").textContent = Math.ceil(butterUsed - playerState.resources.butter) + "g";
        }
        else {
            document.getElementById("bakingbuttershortage").textContent = "";
        }

        if (flourUsed > playerState.resources.flour) {
            document.getElementById("bakingflourshortage").textContent = Math.ceil(flourUsed - playerState.resources.flour) + "g";
        }
        else {
            document.getElementById("bakingflourshortage").textContent = "";
        }

        if (sugarUsed > playerState.resources.sugar) {
            document.getElementById("bakingsugarshortage").textContent = Math.ceil(sugarUsed - playerState.resources.sugar) + "g";
        }
        else {
            document.getElementById("bakingsugarshortage").textContent = "";
        }
    }
}

// -------------------------------------
// ----- STATE: MENU (BUY UPGRADES) ----
// -------------------------------------
function showUpgradeMenu() {
    changeUiState("BUY UPGRADES", "upgrademenu");

    if (playerState.resources.upgradeAllowance > 0) {
        document.getElementById("upgradecredit").textContent = "CREDIT: $" + (playerState.resources.upgradeAllowance / 100).toFixed(2);
    }
    else {
        document.getElementById("upgradecredit").style.display = "none";
    }

    // Clear table
    var upgradeTable = document.getElementById("upgradetable");
    for (var i = upgradeTable.rows.length - 1; i > 1; i--) {
        upgradeTable.deleteRow(i);
    }

    // List upgrades to buy
    for (var i = 0; i < playerState.upgrades.length; i++) {
        var upgrade = playerState.upgrades[i];
        var tr = document.createElement("TR");

        var name = document.createElement("TD");
        name.appendChild(document.createTextNode(upgrade.name));
        tr.appendChild(name);

        var cost = document.createElement("TD");
        cost.appendChild(document.createTextNode("$" + (upgrade.cost / 100).toFixed(2)));
        tr.appendChild(cost);

        var description = document.createElement("TD");
        description.appendChild(document.createTextNode(upgrade.description));
        tr.appendChild(description);

        var totalowned = document.createElement("TD");
        totalowned.appendChild(document.createTextNode(upgrade.amountOwned + playerState.justPurchasedUpgrades[upgrade.name.toLowerCase()]));
        tr.appendChild(totalowned);

        var amounttobuy = document.createElement("TD");
        var amounttobuyinput = document.createElement("input");
        amounttobuyinput.type = "number";
        amounttobuyinput.step = 1;
        amounttobuyinput.min = 0;
        amounttobuyinput.max = 999;
        amounttobuyinput.placeholder = 0;
        amounttobuyinput.id = upgrade.name.toLowerCase() + "buyamount";
        amounttobuyinput.addEventListener("input", function (event) {
            updateUpgradeCost();
        });

        amounttobuy.appendChild(amounttobuyinput);
        tr.appendChild(amounttobuy);

        upgradeTable.appendChild(tr);
    }

    // Add the button to buy upgrades
    var bottomRow = document.createElement("TR");

    var upgradepricesquare = document.createElement("TD");
    upgradepricesquare.colSpan = 2;
    upgradepricesquare.id = "buyupgradeprice";
    bottomRow.appendChild(upgradepricesquare);

    var buyupgradebuttonsquare = document.createElement("TD");
    buyupgradebuttonsquare.colSpan = 3;
    var upgradebuttoninput = document.createElement("input");
    upgradebuttoninput.type = "button";
    upgradebuttoninput.id = "buyupgradesbutton";
    upgradebuttoninput.value = "BUY UPGRADES";
    upgradebuttoninput.className = "display-4";
    upgradebuttoninput.disabled = true;
    upgradebuttoninput.addEventListener("click", function (event) {
        buyUpgrades();
        event.preventDefault();
    });

    buyupgradebuttonsquare.appendChild(upgradebuttoninput);
    bottomRow.appendChild(buyupgradebuttonsquare);

    upgradeTable.appendChild(bottomRow);
}

function getUpgradeCostText(cost, quantity) {
    var costText = "";
    if (cost.item1 > 0) {
        costText = costText + "$" + (cost.item1 * quantity / 100).toFixed(2) + ", ";
    }
    if (cost.item2 > 0) {
        costText = costText + Math.ceil(cost.item2) * quantity + "g Butter, ";
    }
    if (cost.item3 > 0) {
        costText = costText + Math.ceil(cost.item3) * quantity + "g Flour, ";
    }
    if (cost.item4 > 0) {
        costText = costText + Math.ceil(cost.item4) * quantity + "g Sugar, ";
    }
    if (cost.item5 > 0) {
        if (cost.item5 == 1 && quantity == 1) {
            costText = costText + cost.item5 + " Cookie, ";
        } else {
            costText = costText + cost.item5 * quantity + " Cookies, ";
        }
    }
    if (cost.item6 > 0) {
        if (cost.item6 == 1 && quantity == 1) {
            costText = costText + cost.item6 + " Croissant, ";
        } else {
            costText = costText + cost.item6 * quantity + " Croissants, ";
        }
    }
    if (cost.item7 > 0) {
        if (cost.item7 == 1 && quantity == 1) {
            costText = costText + cost.item7 + " Cake, ";
        } else {
            costText = costText + cost.item7 * quantity + " Cakes, ";
        }
    }

    return costText.substring(0, costText.length - 2);
}

function buyUpgrades() {
    var upgradesBought = {};
    for (var i = 0; i < playerState.upgrades.length; i++) {
        var upgrade = playerState.upgrades[i];
        var amountBought = getNumber(upgrade.name.toLowerCase() + "buyamount", true);
        if (amountBought > 0) {
            upgradesBought[upgrade.name.toLowerCase()] = amountBought;
        }
    }

    connection.invoke("BuyUpgrades", roomId, userName, upgradesBought).catch(function (err) {
        return console.error(err.toString());
    });
}

function updateUpgradeCost() {
    var upgradeCost = 0;
    for (var i = 0; i < playerState.upgrades.length; i++) {
        var upgrade = playerState.upgrades[i];
        var amountBought = getNumber(upgrade.name.toLowerCase() + "buyamount", true);
        upgradeCost = upgradeCost + amountBought * upgrade.cost;
    }

    // Early abort if nothing is being bought
    if (upgradeCost <= 0) {
        document.getElementById("buyupgradeprice").textContent = "";
        document.getElementById("buyupgradesbutton").disabled = true;
        document.getElementById("buyupgradesbutton").value = "BUY UPGRADES";
        return;
    }

    if (playerState.resources.upgradeAllowance > 0) {
        if (playerState.resources.upgradeAllowance >= upgradeCost) {
            document.getElementById("buyupgradeprice").textContent = "$" + (upgradeCost / 100).toFixed(2) + "/$" + (playerState.resources.upgradeAllowance / 100).toFixed(2) + " CREDIT";
        }
        else {
            var cashNeeded = upgradeCost - playerState.resources.upgradeAllowance;
            var upgradeCreditUsed = "$" + (playerState.resources.upgradeAllowance / 100).toFixed(2);
            document.getElementById("buyupgradeprice").textContent = upgradeCreditUsed + "/" + upgradeCreditUsed + " CREDIT + $" + (cashNeeded / 100).toFixed(2) + "/$" + (playerState.resources.money / 100).toFixed(2);
        }
    }
    else {
        document.getElementById("buyupgradeprice").textContent = "$" + (upgradeCost / 100).toFixed(2) + "/$" + (playerState.resources.money / 100).toFixed(2);
    }    

    if (upgradeCost > playerState.resources.money + playerState.resources.upgradeAllowance) {
        document.getElementById("buyupgradeprice").style.color = "red";
        document.getElementById("buyupgradesbutton").disabled = true;
        document.getElementById("buyupgradesbutton").value = "NOT ENOUGH MONEY";
    }
    else {
        document.getElementById("buyupgradeprice").style.color = "blue";
        document.getElementById("buyupgradesbutton").disabled = false;
        document.getElementById("buyupgradesbutton").value = "BUY UPGRADES";
    }
}

// -------------------------------------
// ----- STATE: MENU (USE UPGRADES) ----
// -------------------------------------
function showUseUpgradeMenu() {
    changeUiState("USE UPGRADES", "useupgradesmenu");

    // Clear the table
    var useUpgradeTable = document.getElementById("useupgradetable");
    for (var i = useUpgradeTable.rows.length - 1; i > 1; i--) {
        useUpgradeTable.deleteRow(i);
    }

    // List upgrades to use
    var hasUpgradesToUse = false;
    for (var i = 0; i < playerState.upgrades.length; i++) {
        var upgrade = playerState.upgrades[i];
        if (upgrade.usable && upgrade.amountOwned > 0) {
            hasUpgradesToUse = true;
            var tr = document.createElement("TR");

            var name = document.createElement("TD");
            name.appendChild(document.createTextNode(upgrade.name));
            tr.appendChild(name);

            var totalowned = document.createElement("TD");
            totalowned.appendChild(document.createTextNode(upgrade.amountOwned));
            tr.appendChild(totalowned);

            var usesleft = document.createElement("TD");
            if (upgrade.usesLeft < 0) {
                usesleft.appendChild(document.createTextNode("Infinite"));
            } else {
                usesleft.appendChild(document.createTextNode(upgrade.usesLeft));
            }
            tr.appendChild(usesleft);

            var upgradecost = document.createElement("TD");
            upgradecost.id = upgrade.name.toLowerCase() + "usagecost";
            upgradecost.appendChild(document.createTextNode(getUpgradeCostText(upgrade.useCost, 1) + "/Use"));
            tr.appendChild(upgradecost);

            var upgradeeffect = document.createElement("TD");
            upgradeeffect.id = upgrade.name.toLowerCase() + "usageeffect";
            upgradeeffect.appendChild(document.createTextNode(getUpgradeCostText(upgrade.useEffect, 1) + "/Use"));
            tr.appendChild(upgradeeffect);

            var amounttouse = document.createElement("TD");
            var amounttouseinput = document.createElement("input");
            amounttouseinput.type = "number";
            amounttouseinput.step = 1;
            amounttouseinput.min = 0;

            if (upgrade.usesLeft >= 0) {
                amounttouseinput.max = upgrade.usesLeft;
            }
            else {
                amounttouseinput.max = 999;
            }

            amounttouseinput.placeholder = 0;
            amounttouseinput.id = upgrade.name.toLowerCase() + "useamount";
            amounttouseinput.upgradeName = upgrade.name.toLowerCase();
            amounttouseinput.useCost = upgrade.useCost;
            amounttouseinput.useEffect = upgrade.useEffect;

            amounttouseinput.addEventListener("input", function () {
                updateUpgradeUse(this.upgradeName, this.useCost, this.useEffect);
            });

            amounttouse.appendChild(amounttouseinput);
            tr.appendChild(amounttouse);

            var useupgradebuttonsquare = document.createElement("TD");
            var useupgradebuttoninput = document.createElement("input");
            useupgradebuttoninput.type = "button";
            useupgradebuttoninput.id = upgrade.name.toLowerCase() + "use";
            useupgradebuttoninput.value = "USE";
            useupgradebuttoninput.disabled = true;
            useupgradebuttoninput.upgradeName = upgrade.name.toLowerCase();
            useupgradebuttoninput.addEventListener("click", function () {
                useUpgrade(this.upgradeName);
                event.preventDefault();
            });

            useupgradebuttonsquare.appendChild(useupgradebuttoninput);
            tr.appendChild(useupgradebuttonsquare);

            useUpgradeTable.appendChild(tr);
        }
    }

    // List message when there are no upgrades that are usable
    if (!hasUpgradesToUse) {
        var tr = document.createElement("TR");

        var buyupgradetousemessage = document.createElement("TD");
        buyupgradetousemessage.colSpan = 5;
        buyupgradetousemessage.appendChild(document.createTextNode("You don't have any usable upgrades. Buy some above. Upgrades only come into play the round after they are purchased."));
        tr.appendChild(buyupgradetousemessage);
        useUpgradeTable.appendChild(tr);
    }
}

function useUpgrade(upgrade) {
    var numberOfUses = getNumber(upgrade + "useamount", true);

    // Don't check costs here
    connection.invoke("UseUpgrade", roomId, userName, upgrade, numberOfUses).catch(function (err) {
        return console.error(err.toString());
    });
}

function updateUpgradeUse(upgrade, useCost, useEffect) {
    var numberOfUses = getNumber(upgrade + "useamount", true);

    if (numberOfUses == 0) {
        document.getElementById(upgrade + "usagecost").textContent = getUpgradeCostText(useCost, 1) + "/Use";
        document.getElementById(upgrade + "usageeffect").textContent = getUpgradeCostText(useEffect, 1) + "/Use";
        document.getElementById(upgrade + "usagecost").style.color = "black";
        document.getElementById(upgrade + "usageeffect").style.color = "black";
        document.getElementById(upgrade + "use").disabled = true;
        return;
    }

    var usable = true;

    document.getElementById(upgrade + "usagecost").textContent = getUpgradeCostText(useCost, numberOfUses);
    document.getElementById(upgrade + "usageeffect").textContent = getUpgradeCostText(useEffect, numberOfUses);

    // Calculate whether there are enough resources and reflect changes in UI color and button text
    if (useCost.item1 > 0 && useCost.item1 * numberOfUses > playerState.resources.money) {
        usable = false;
        document.getElementById(upgrade + "use").value = "LACK MONEY";
    }
    else if (useCost.item2 > 0 && useCost.item2 * numberOfUses > playerState.resources.butter) {
        usable = false;
        document.getElementById(upgrade + "use").value = "LACK BUTTER";
    }
    else if (useCost.item3 > 0 && useCost.item3 * numberOfUses > playerState.resources.flour) {
        usable = false;
        document.getElementById(upgrade + "use").value = "LACK FLOUR";
    }
    else if (useCost.item4 > 0 && useCost.item4 * numberOfUses > playerState.resources.sugar) {
        usable = false;
        document.getElementById(upgrade + "use").value = "LACK SUGAR";
    }
    else if (useCost.item5 > 0 && useCost.item5 * numberOfUses > playerState.bakedGoods.cookies) {
        usable = false;
        document.getElementById(upgrade + "use").value = "NEED COOKIES";
    }
    else if (useCost.item6 > 0 && useCost.item6 * numberOfUses > playerState.bakedGoods.croissants) {
        usable = false;
        document.getElementById(upgrade + "use").value = "NEED CROISSANTS";
    }
    else if (useCost.item7 > 0 && useCost.item7 * numberOfUses > playerState.bakedGoods.cakes) {
        usable = false;
        document.getElementById(upgrade + "use").value = "NEED CAKES";
    }

    if (usable) {
        document.getElementById(upgrade + "usagecost").style.color = "blue";
        document.getElementById(upgrade + "usageeffect").style.color = "blue";
        document.getElementById(upgrade + "use").disabled = false;
        document.getElementById(upgrade + "use").value = "USE";
    }
    else {
        document.getElementById(upgrade + "usagecost").style.color = "red";
        document.getElementById(upgrade + "usageeffect").style.color = "red";
        document.getElementById(upgrade + "use").disabled = true;
    }
}

// -------------------------
// ----- STATE: SELLING ----
// -------------------------
function summarizeGoodsBaked() {
    if (playerState.resources.upgradeAllowance > 0) {
        document.getElementById("upgradecreditwarning").textContent = "Note: You have ended your turn with $" + (playerState.resources.upgradeAllowance / 100).toFixed(2) + " of unspent Upgrade Credit.";
    }
    else {
        document.getElementById("upgradecreditwarning").style.display = "none";
    }

    changeUiState("SET UP SHOP", "goodsbaked");
    document.getElementById("currentyearsummary").textContent = "ROUND: " + (gameState.currentMarket.currentYear + 1) + "/" + gameState.currentMarket.maxYears;

    document.getElementById("cookiesbeingsold").textContent = playerState.bakedGoods.cookies;
    document.getElementById("croissantsbeingsold").textContent = playerState.bakedGoods.croissants;
    document.getElementById("cakesbeingsold").textContent = playerState.bakedGoods.cakes;

    var upgradesBoughtList = document.getElementById("upgradesboughtlist");
    upgradesBoughtList.innerHTML = "";
    var upgradesWerePurchased = false;
    for (var justPurchasedUpgrade in playerState.justPurchasedUpgrades) {
        var amountPurchased = playerState.justPurchasedUpgrades[justPurchasedUpgrade];
        if (amountPurchased > 0) {
            upgradesWerePurchased = true;
            var li = document.createElement("li");
            li.textContent = amountPurchased + "x " + justPurchasedUpgrade.toUpperCase();
            upgradesBoughtList.appendChild(li);
        }
    }

    if (!upgradesWerePurchased) {
        document.getElementById("upgradesboughtlistdiv").style.display = "none";
    } else {
        document.getElementById("upgradesboughtlistdiv").style.display = "block";
    }
}

document.getElementById("gobacktobakeviewbutton").addEventListener("click", function (event) {
    connection.invoke("BackToBakery", roomId, userName).catch(function (err) {
        return console.error(err.toString());
    });

    showMenu();
    event.preventDefault();
});

// Go to set up shop screen - ready to end turn
document.getElementById("endturnbutton").addEventListener("click", function (event) {
    summarizeGoodsBaked();
    connection.invoke("SetUpShop", roomId, userName).catch(function (err) {
        return console.error(err.toString());
    });

    event.preventDefault();
});

// -------------------------
// ----- STATE: WAITING ----
// -------------------------
connection.on("SetUpShop", function (currentPrices, currentMarket, playerResources, playerUpgrades, playerBakedGoods, readyPlayers, notReadyPlayers) {
    gameState.currentPrices = currentPrices;
    gameState.currentMarket = currentMarket;
    playerState.resources = playerResources;
    playerState.upgrades = playerUpgrades;
    playerState.bakedGoods = playerBakedGoods;

    summarizeGoodsBaked();
    updatePlayerList(readyPlayers, notReadyPlayers);
});

connection.on("UpdatePlayerList", function (readyPlayers, notReadyPlayers) {
    updatePlayerList(readyPlayers, notReadyPlayers);
})

function updatePlayerList(readyPlayers, slowBastards) {
    var readyList = document.getElementById("playerreadylist");
    readyList.innerHTML = "";
    for (let i = 0; i < readyPlayers.length; i++) {
        var li = document.createElement("li");
        li.textContent = readyPlayers[i];
        readyList.appendChild(li);
    }

    var slowBastardList = document.getElementById("playerwaitinglist");
    slowBastardList.innerHTML = "";
    for (let i = 0; i < slowBastards.length; i++) {
        var li = document.createElement("li");
        li.textContent = slowBastards[i];
        slowBastardList.appendChild(li);
    }
}

// ------------------------------
// ----- STATE: MARKET REPORT ---
// ------------------------------
connection.on("ShowMarketReport", function (marketReport, currentPlayerSales, goodPrices, playerProfit, currentMarket, playerIngredientRefund, playerUpgradeReport, newsReport) {
    gameState.currentMarket = currentMarket;
    bakingMenuState = "buyingingredients";
    changeUiState("MARKET REPORT", "marketreport");

    document.getElementById("marketreportnews").textContent = newsReport;
    var marketReportTable = document.getElementById("marketreporttable");
    for (var i = marketReportTable.rows.length - 1; i > 1; i--) {
        marketReportTable.deleteRow(i);
    }

    for (var playerName in marketReport) {
        var playerSales = marketReport[playerName];
        var cookiesSold = playerSales.item1;
        var croissantsSold = playerSales.item2;
        var cakesSold = playerSales.item3;

        var tr = document.createElement("TR");

        var name = document.createElement("TD");
        name.appendChild(document.createTextNode(playerName.toUpperCase()));
        tr.appendChild(name);

        var cookie = document.createElement("TD");
        cookie.appendChild(document.createTextNode(cookiesSold));
        tr.appendChild(cookie);

        var croissant = document.createElement("TD");
        croissant.appendChild(document.createTextNode(croissantsSold));
        tr.appendChild(croissant);

        var cake = document.createElement("TD");
        cake.appendChild(document.createTextNode(cakesSold));
        tr.appendChild(cake);

        var salesNumber = (cookiesSold * goodPrices.item1) + (croissantsSold * goodPrices.item2) + (cakesSold * goodPrices.item3);
        var sales = document.createElement("TD");
        sales.appendChild(document.createTextNode("$" + (salesNumber/100).toFixed(2)));
        tr.appendChild(sales);

        marketReportTable.appendChild(tr);
    }

    document.getElementById("marketreporttitle").textContent = "ROUND: " + gameState.currentMarket.currentYear + "/"
        + gameState.currentMarket.maxYears + " SALES REPORT";

    if (currentPlayerSales.item1 > 0) {
        document.getElementById("marketreportcookieprice").textContent = "$" + (goodPrices.item1 / 100).toFixed(2)
        document.getElementById("marketreportcookieamount").textContent = currentPlayerSales.item1;
        document.getElementById("marketreportcookierevenue").textContent = "$" + (currentPlayerSales.item1 * goodPrices.item1 / 100).toFixed(2);
    }
    else {
        document.getElementById("marketreportcookieprice").textContent = ""
        document.getElementById("marketreportcookieamount").textContent = ""
        document.getElementById("marketreportcookierevenue").textContent = ""
    }

    if (currentPlayerSales.item2 > 0) {
        document.getElementById("marketreportcroissantprice").textContent = "$" + (goodPrices.item2 / 100).toFixed(2)
        document.getElementById("marketreportcroissantamount").textContent = currentPlayerSales.item2;
        document.getElementById("marketreportcroissantrevenue").textContent = "$" + (currentPlayerSales.item2 * goodPrices.item2 / 100).toFixed(2);
    }
    else {
        document.getElementById("marketreportcroissantprice").textContent = ""
        document.getElementById("marketreportcroissantamount").textContent = ""
        document.getElementById("marketreportcroissantrevenue").textContent = ""
    }

    if (currentPlayerSales.item3 > 0) {
        document.getElementById("marketreportcakeprice").textContent = "$" + (goodPrices.item3 / 100).toFixed(2)
        document.getElementById("marketreportcakeamount").textContent = currentPlayerSales.item3;
        document.getElementById("marketreportcakerevenue").textContent = "$" + (currentPlayerSales.item3 * goodPrices.item3 / 100).toFixed(2);
    }
    else {
        document.getElementById("marketreportcakeprice").textContent = ""
        document.getElementById("marketreportcakeamount").textContent = ""
        document.getElementById("marketreportcakerevenue").textContent = ""
    }

    document.getElementById("salestablesummary").textContent = "TOTAL REVENUE: $" + (playerProfit / 100).toFixed(2);

    var upgradesBoughtList = document.getElementById("upgradesboughtlistmarketreport");
    upgradesBoughtList.innerHTML = "";
    var upgradesWerePurchased = false;
    for (var justPurchasedUpgrade in playerState.justPurchasedUpgrades) {
        var amountPurchased = playerState.justPurchasedUpgrades[justPurchasedUpgrade];
        if (amountPurchased > 0) {
            upgradesWerePurchased = true;
            var li = document.createElement("li");
            li.textContent = amountPurchased + "x " + justPurchasedUpgrade.toUpperCase();
            upgradesBoughtList.appendChild(li);
        }
    }

    if (!upgradesWerePurchased) {
        document.getElementById("upgradesboughtlistmarketreportdiv").style.display = "none";
    } else {
        document.getElementById("upgradesboughtlistmarketreportdiv").style.display = "block";
    }

    var otherIncomeText = "";
    if (playerIngredientRefund > 0.1) {
        otherIncomeText = "Sold unused ingredients for $" + (playerIngredientRefund / 100).toFixed(2) + ".";
    }

    if (currentMarket.annualAllowance > 0.1) {
        otherIncomeText = otherIncomeText + " The state gives you an annual allowance of $" + (currentMarket.annualAllowance / 100).toFixed(2) + ".";
    }

    document.getElementById("otherincomesources").textContent = otherIncomeText;
    document.getElementById("upgradesmarketreport").textContent = playerUpgradeReport;
});

// Stop viewing market report
document.getElementById("endmarketreportbutton").addEventListener("click", function (event) {
    connection.invoke("EndMarketReport", roomId, userName).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

// -------------------------
// ----- STATE: ENDGAME ----
// -------------------------
connection.on("EndGame", function (totalSales, playerSales) {
    changeUiState("END OF GAME", "endgame");

    var leaderboard = document.getElementById("endgameleaderboard");

    for (var i = 0; i < totalSales.length; i++) {
        var tr = document.createElement("TR");

        var name = document.createElement("TD");
        name.appendChild(document.createTextNode(totalSales[i].item1.toUpperCase()));
        tr.appendChild(name);
        var cookies = document.createElement("TD");
        cookies.appendChild(document.createTextNode(totalSales[i].item2));
        tr.appendChild(cookies);
        var croissants = document.createElement("TD");
        croissants.appendChild(document.createTextNode(totalSales[i].item3));
        tr.appendChild(croissants);
        var cakes = document.createElement("TD");
        cakes.appendChild(document.createTextNode(totalSales[i].item4));
        tr.appendChild(cakes);
        var total = document.createElement("TD");
        total.appendChild(document.createTextNode("$" + (totalSales[i].item5 / 100).toFixed(2)));
        tr.appendChild(total);
        leaderboard.appendChild(tr);
    }

    var annualreport = document.getElementById("annualreport");
    for (var i = 0; i < playerSales.length; i++) {
        var tr = document.createElement("TR");

        var year = document.createElement("TD");
        year.appendChild(document.createTextNode(i + 1));
        tr.appendChild(year);

        var cookies = document.createElement("TD");
        cookies.appendChild(document.createTextNode(playerSales[i].item1));
        tr.appendChild(cookies);
        var croissants = document.createElement("TD");
        croissants.appendChild(document.createTextNode(playerSales[i].item2));
        tr.appendChild(croissants);
        var cakes = document.createElement("TD");
        cakes.appendChild(document.createTextNode(playerSales[i].item3));
        tr.appendChild(cakes);
        var total = document.createElement("TD");
        total.appendChild(document.createTextNode("$" + (playerSales[i].item4 / 100).toFixed(2)));
        tr.appendChild(total);

        annualreport.appendChild(tr);
    }
});

// Stop viewing market report
document.getElementById("exitgamebutton").addEventListener("click", function (event) {
    sessionStorage.removeItem("username");
    sessionStorage.removeItem("roomid");
    location.reload();
    event.preventDefault();
});