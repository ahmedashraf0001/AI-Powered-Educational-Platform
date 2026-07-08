import 'package:flutter/material.dart';
import 'package:graduation_app/core/networking/api_constants.dart';
import 'package:syncfusion_flutter_pdfviewer/pdfviewer.dart';
import '../../../../core/di/dependency_injection.dart';
import '../../../../core/helpers/secure_storage_helper.dart';

class PdfPreviewWidget extends StatefulWidget {
  final String pdfUrl;

  const PdfPreviewWidget({super.key, required this.pdfUrl});

  @override
  State<PdfPreviewWidget> createState() => _PdfPreviewWidgetState();
}

class _PdfPreviewWidgetState extends State<PdfPreviewWidget> {
  String? token;

  @override
  void initState() {
    super.initState();
    loadToken();
  }

  Future<void> loadToken() async {
    final savedToken = await getIt<SecureStorageHelper>().getToken(
      key: ApiKeys.token,
    );

    setState(() {
      token = savedToken;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (token == null) {
      return const Center(child: CircularProgressIndicator());
    }

    return SfPdfViewer.network(
      '${ApiConstants.pdfBaseUrl}${widget.pdfUrl}',
      headers: {'Authorization': 'Bearer $token'},
    );
  }
}
