import 'package:web/web.dart' as web;

void requestWindowsMatlab(bool selectFile) {
  // Keep this synchronous: browsers require a direct user gesture for opening
  // an installed protocol handler. No file paths or MATLAB code enter the URI.
  web.window.location.href =
      'logiverse-matlab://${selectFile ? 'cargar' : 'fuzzy'}';
}

void downloadWindowsConnector(Uri uri) {
  final link = web.HTMLAnchorElement()
    ..href = uri.toString()
    ..download = 'Conector-MATLAB-Windows.zip';
  web.document.body!.append(link);
  link.click();
  link.remove();
}
