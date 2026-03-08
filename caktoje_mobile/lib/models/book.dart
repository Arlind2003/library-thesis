import 'app_image.dart';

class Book {
  final int id;
  final String title;
  final String description;
  final String isbn;
  final AppImage image;
  final List<Author> authors;
  final List<Category> categories;
  final List<BookStock> bookStocks;

  Book({
    required this.id,
    required this.title,
    required this.description,
    required this.isbn,
    required this.image,
    required this.authors,
    required this.categories,
    required this.bookStocks,
  });

  factory Book.fromJson(Map<String, dynamic> json) {
    return Book(
      id: json['id'],
      title: json['name'],
      description: json['description'],
      isbn: json['isbn'],
      image: AppImage.fromJson(json['image']),
      authors: (json['authors'] as List)
          .map((author) => Author.fromJson(author))
          .toList(),
      categories: (json['categories'] as List)
          .map((category) => Category.fromJson(category))
          .toList(),
      bookStocks: (json['bookStocks'] as List)
          .map((stock) => BookStock.fromJson(stock))
          .toList(),
    );
  }
}

class Author {
  final int id;
  final String name;
  final String? biography;
  final AppImage? image;

  Author({required this.id, required this.name, this.biography, this.image});

  factory Author.fromJson(Map<String, dynamic> json) {
    return Author(
      id: json['id'],
      name: json['name'],
      biography: json['biography'],
      image: json['image'] != null ? AppImage.fromJson(json['image']) : null,
    );
  }
}

class Category {
  final int id;
  final String name;
  final AppImage? image;

  Category({required this.id, required this.name, this.image});

  factory Category.fromJson(Map<String, dynamic> json) {
    return Category(
      id: json['id'],
      name: json['name'],
      image: json['image'] != null ? AppImage.fromJson(json['image']) : null,
    );
  }
}

class BookStock {
  final int id;
  final String location;
  final String state;

  BookStock({required this.id, required this.location, required this.state});

  factory BookStock.fromJson(Map<String, dynamic> json) {
    return BookStock(
      id: json['id'],
      location: '${json['putwall']} (${json['row']}-${json['column']})',
      state: json['state'],
    );
  }
}
