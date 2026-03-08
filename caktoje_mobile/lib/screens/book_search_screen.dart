import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../models/book.dart';
import '../models/paginated_resource.dart';
import '../services/book_service.dart';
import '../services/author_service.dart';
import '../services/category_service.dart';
import '../widgets/book_card.dart';
import 'package:multi_select_flutter/multi_select_flutter.dart';

class BookSearchScreen extends StatefulWidget {
  const BookSearchScreen({Key? key}) : super(key: key);

  @override
  _BookSearchScreenState createState() => _BookSearchScreenState();
}

class _BookSearchScreenState extends State<BookSearchScreen> {
  final BookService _bookService = BookService();
  final AuthorService _authorService = AuthorService();
  final CategoryService _categoryService = CategoryService();

  String _query = '';
  List<int> _selectedCategoryIds = [];
  List<int> _selectedAuthorIds = [];
  int _page = 1;
  final int _pageSize = 10;

  Future<PaginatedResource<Book>>? _booksFuture;
  Future<PaginatedResource<Author>>? _authorsFuture;
  Future<PaginatedResource<Category>>? _categoriesFuture;

  @override
  void initState() {
    super.initState();
    _fetchData();
  }

  void _fetchData() {
    setState(() {
      _booksFuture = _bookService.getBooks(
        _query,
        _selectedCategoryIds,
        _selectedAuthorIds,
        _page,
        _pageSize,
      );
      _authorsFuture = _authorService.getAuthors('', 1, 1000);
      _categoriesFuture = _categoryService.getCategories('', 1, 1000);
    });
  }

  @override
  Widget build(BuildContext context) {
    return Theme(
      data: ThemeData.dark().copyWith(
        scaffoldBackgroundColor: const Color(0xFF212121),
        textTheme: GoogleFonts.poppinsTextTheme(ThemeData.dark().textTheme),
        colorScheme: const ColorScheme.dark(
          primary: Colors.deepPurpleAccent,
          secondary: Colors.deepPurple,
        ),
      ),
      child: Scaffold(
        appBar: AppBar(
          backgroundColor: Colors.transparent,
          elevation: 0,
          title: Text(
            'Dukagjini',
            style: GoogleFonts.pacifico(
              fontSize: 28,
              fontWeight: FontWeight.bold,
            ),
          ),
        ),
        body: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildSearchBar(),
              const SizedBox(height: 16),
              _buildFilters(),
              const SizedBox(height: 16),
              Expanded(
                child: FutureBuilder<PaginatedResource<Book>>(
                  future: _booksFuture,
                  builder: (context, snapshot) {
                    if (snapshot.connectionState == ConnectionState.waiting) {
                      return const Center(child: CircularProgressIndicator());
                    } else if (snapshot.hasError) {
                      return Center(
                        child: Text(
                          'Error: ${snapshot.error}\n${snapshot.stackTrace}',
                        ),
                      );
                    } else if (!snapshot.hasData ||
                        snapshot.data!.items.isEmpty) {
                      return const Center(child: Text('No books found.'));
                    }

                    final books = snapshot.data!.items;
                    return ListView.builder(
                      itemCount: books.length,
                      itemBuilder: (context, index) {
                        return Padding(
                          padding: const EdgeInsets.only(bottom: 16.0),
                          child: BookCard(book: books[index]),
                        );
                      },
                    );
                  },
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildSearchBar() {
    return TextField(
      onChanged: (value) {
        setState(() {
          _query = value;
        });
      },
      onSubmitted: (value) => _fetchData(),
      decoration: InputDecoration(
        hintText: 'Search for books...',
        prefixIcon: const Icon(Icons.search),
        filled: true,
        fillColor: Colors.grey[800],
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12.0),
          borderSide: BorderSide.none,
        ),
      ),
    );
  }

  Widget _buildFilters() {
    return Row(
      children: [
        Expanded(
          child: FutureBuilder<PaginatedResource<Category>>(
            future: _categoriesFuture,
            builder: (context, snapshot) {
              if (!snapshot.hasData) return Container();
              final categories = snapshot.data!.items;
              return _buildMultiSelect(
                "Categories",
                categories.map((c) => MultiSelectItem(c.id, c.name)).toList(),
                (values) {
                  setState(() {
                    _selectedCategoryIds = values.cast<int>();
                  });
                  _fetchData();
                },
                _selectedCategoryIds,
              );
            },
          ),
        ),
        const SizedBox(width: 16),
        Expanded(
          child: FutureBuilder<PaginatedResource<Author>>(
            future: _authorsFuture,
            builder: (context, snapshot) {
              if (!snapshot.hasData) return Container();
              final authors = snapshot.data!.items;
              return _buildMultiSelect(
                "Authors",
                authors.map((a) => MultiSelectItem(a.id, a.name)).toList(),
                (values) {
                  setState(() {
                    _selectedAuthorIds = values.cast<int>();
                  });
                  _fetchData();
                },
                _selectedAuthorIds,
              );
            },
          ),
        ),
      ],
    );
  }

  Widget _buildMultiSelect<T>(
    String title,
    List<MultiSelectItem<T>> items,
    void Function(List<T>) onConfirm,
    List<T> initialValue,
  ) {
    return MultiSelectDialogField<T>(
      items: items,
      initialValue: initialValue,
      title: Text(title, style: const TextStyle(color: Colors.white)),
      buttonText: Text(title, style: const TextStyle(color: Colors.white)),
      buttonIcon: const Icon(Icons.filter_list, color: Colors.white),
      decoration: BoxDecoration(
        color: Colors.grey[800],
        borderRadius: BorderRadius.circular(12.0),
      ),
      onConfirm: onConfirm,
      chipDisplay: MultiSelectChipDisplay.none(),
      itemsTextStyle: const TextStyle(color: Colors.white),
      selectedItemsTextStyle: const TextStyle(color: Colors.deepPurpleAccent),
      unselectedColor: Colors.grey[800],
      selectedColor: Colors.deepPurpleAccent.withOpacity(0.1),
      backgroundColor: Colors.grey[900],
      searchable: true,
      searchHint: 'Search $title',
      searchIcon: const Icon(Icons.search, color: Colors.white),
      searchHintStyle: const TextStyle(color: Colors.white70),
      searchTextStyle: const TextStyle(color: Colors.white),
    );
  }
}
