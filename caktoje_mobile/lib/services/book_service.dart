import 'dart:convert';
import 'package:http/http.dart' as http;
import '../constants/api.dart';
import '../models/book.dart';
import '../models/paginated_resource.dart';

class BookService {
  Future<PaginatedResource<Book>> getBooks(
    String query,
    List<int> categoryIds,
    List<int> authorIds,
    int page,
    int pageSize,
  ) async {
    final params = {
      'query': query,
      'categoryIds': categoryIds.map((id) => id.toString()).toList(),
      'authorIds': authorIds.map((id) => id.toString()).toList(),
      'page': page.toString(),
      'pageSize': pageSize.toString(),
    };

    final uri = Uri.parse(
      '$apiUrl/admin/books',
    ).replace(queryParameters: params);
    final response = await http.get(uri);

    if (response.statusCode == 200) {
      final data = json.decode(response.body);
      print(data);

      return PaginatedResource.fromJson(data, (item) => Book.fromJson(item));
    } else {
      throw Exception('Failed to load books');
    }
  }
}
