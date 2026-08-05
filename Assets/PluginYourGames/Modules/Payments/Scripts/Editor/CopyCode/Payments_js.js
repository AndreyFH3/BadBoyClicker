var paymentsData = NO_DATA;
let payments = null;

async function InitPayments(sendback) {
    let result = NO_DATA;

    try {
        if (ysdk == null)
            return Final(result);

        payments = await ysdk.getPayments();
        const products = await payments.getCatalog();
        const purchases = await payments.getPurchases();
        let productID = [];
        let title = [];
        let description = [];
        let imageURI = [];
        let price = [];
        let priceValue = [];
        let priceCurrencyCode = [];
        let currencyImageURL = [];
        let consumed = [];

        for (let i = 0; i < products.length; i++) {
            productID[i] = products[i].id;
            title[i] = products[i].title;
            description[i] = products[i].description;
            imageURI[i] = products[i].imageURI;
            price[i] = products[i].price;
            priceValue[i] = products[i].priceValue;
            priceCurrencyCode[i] = products[i].priceCurrencyCode;
            currencyImageURL[i] = products[i].getPriceCurrencyImage("medium");
            consumed[i] = !purchases.some(purchase => purchase.productID === productID[i]);
        }

        result = JSON.stringify({
            "id": productID,
            "title": title,
            "description": description,
            "imageURI": imageURI,
            "price": price,
            "priceValue": priceValue,
            "priceCurrencyCode": priceCurrencyCode,
            "currencyImageURL": currencyImageURL,
            "consumed": consumed,
            "language": ysdk.environment.i18n.lang
        });
    } catch (e) {
        console.error('Purchases are not available', e);
        payments = null;
    }

    return Final(result);

    function Final(res) {
        paymentsData = res;
        YG2Instance('PaymentsEntries', res);
        return res;
    }
}

async function BuyPayments(id) {
    try {
        if (payments == null)
            throw new Error('Payments are not initialized');

        await payments.purchase(id);
        LogStyledMessage('Purchase Success');
        YG2Instance('OnPurchaseSuccess', id);
    }
    catch (e) {
        console.error('Purchase Failed', e);
        YG2Instance('OnPurchaseFailed', id);
    }
    finally {
        FocusGame();
    }
}

async function ConsumePurchase(id, onPurchaseSuccess) {
    try {
        if (payments == null)
            throw new Error('Payments are not initialized');

        const purchases = await payments.getPurchases();
        const purchase = purchases.find(entry => entry.productID === id);
        if (purchase == null)
            throw new Error('Unprocessed purchase not found: ' + id);

        await payments.consumePurchase(purchase.purchaseToken);

        if (onPurchaseSuccess)
            YG2Instance('OnPurchaseSuccess', id);
    } catch (e) {
        console.error('Consume purchase failed', id, e);
    }
}

async function ConsumePurchases(onPurchaseSuccess) {
    try {
        if (payments == null)
            throw new Error('Payments are not initialized');

        const purchases = await payments.getPurchases();
        LogStyledMessage('Unprocessed purchases: ', purchases.length);
        for (let i = 0; i < purchases.length; i++) {
            if (onPurchaseSuccess)
                YG2Instance('OnPurchaseSuccess', purchases[i].productID);
            else
                await payments.consumePurchase(purchases[i].purchaseToken);
        }
    } catch (e) {
        console.error('Consume purchases failed', e);
    }
}
