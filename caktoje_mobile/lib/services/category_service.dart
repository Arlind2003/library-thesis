import 'dart:convert';
import 'package:http/http.dart' as http;
import '../constants/api.dart';
import '../models/book.dart';
import '../models/paginated_resource.dart';

class CategoryService {
  Future<PaginatedResource<Category>> getCategories(String query, int page, int pageSize) async {
    final params = {
      'query': query,
      'page': page.toString(),
      'pageSize': pageSize.toString(),
    };

    final uri = Uri.parse('$apiUrl/admin/categories').replace(queryParameters: params);
    final response = await http.get(uri);

    if (response.statusCode == 200) {
      final data = json.decode(response.body);
      return PaginatedResource.fromJson(data, (item) => Category.fromJson(item));
    } else {
      throw Exception('Failed to load categories');
    }
  }
}
