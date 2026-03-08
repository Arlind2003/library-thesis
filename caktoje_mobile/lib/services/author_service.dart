import 'dart:convert';
import 'package:http/http.dart' as http;
import '../constants/api.dart';
import '../models/book.dart';
import '../models/paginated_resource.dart';

class AuthorService {
  Future<PaginatedResource<Author>> getAuthors(String query, int page, int pageSize) async {
    final params = {
      'query': query,
      'page': page.toString(),
      'pageSize': pageSize.toString(),
    };

    final uri = Uri.parse('$apiUrl/admin/authors').replace(queryParameters: params);
    final response = await http.get(uri);

    if (response.statusCode == 200) {
      final data = json.decode(response.body);
      return PaginatedResource.fromJson(data, (item) => Author.fromJson(item));
    } else {
      throw Exception('Failed to load authors');
    }
  }
}
