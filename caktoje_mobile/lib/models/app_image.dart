class AppImage {
  final String fileName;

  AppImage({required this.fileName});

  factory AppImage.fromJson(Map<String, dynamic> json) {
    return AppImage(fileName: json['fileName']);
  }
}
