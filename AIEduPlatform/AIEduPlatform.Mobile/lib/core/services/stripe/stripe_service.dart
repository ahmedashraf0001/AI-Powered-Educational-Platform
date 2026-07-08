import 'package:flutter_stripe/flutter_stripe.dart';

class StripeService {

  Future<void> initStripe(String publishableKey) async {
    Stripe.publishableKey = publishableKey;
    await Stripe.instance.applySettings();
  }

  Future<void> makePayment(String clientSecret) async {

    await Stripe.instance.initPaymentSheet(
      paymentSheetParameters: SetupPaymentSheetParameters(
        paymentIntentClientSecret: clientSecret,
        merchantDisplayName: 'Courses App',
      ),
    );

    await Stripe.instance.presentPaymentSheet();
  }
}