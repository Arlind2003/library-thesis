class PaginatedResource<T> {
  final List<T> items;
  final int totalPages;

  PaginatedResource({required this.items, required this.totalPages});

  factory PaginatedResource.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic>) fromJsonT,
  ) {
    return PaginatedResource(
      items: (json['items'] as List).map((item) => fromJsonT(item)).toList(),
      totalPages: json['totalPages'],
    );
  }
}
