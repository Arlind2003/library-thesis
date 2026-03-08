import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../models/book.dart';

class BookDetailScreen extends StatelessWidget {
  final Book book;

  const BookDetailScreen({super.key, required this.book});

  @override
  Widget build(BuildContext context) {
    final groupedStock = _groupBookStocks();
    final imageUrl =
        'http://localhost:5173/Files/Images/${book.image.fileName}';

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
        body: CustomScrollView(
          slivers: [
            SliverAppBar(
              expandedHeight: 300.0,
              pinned: true,
              flexibleSpace: FlexibleSpaceBar(
                title: Text(
                  book.title,
                  style: GoogleFonts.poppins(
                    fontWeight: FontWeight.bold,
                    fontSize: 16,
                  ),
                ),
                background: Image.network(
                  imageUrl,
                  fit: BoxFit.cover,
                  errorBuilder: (context, error, stackTrace) {
                    return Container(
                      color: Colors.grey[800],
                      child: const Icon(
                        Icons.book,
                        size: 100,
                        color: Colors.grey,
                      ),
                    );
                  },
                ),
              ),
            ),
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    _buildInfoSection(),
                    const SizedBox(height: 24),
                    _buildDescriptionSection(context),
                    const SizedBox(height: 24),
                    _buildStockSection(context, groupedStock),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Map<String, int> _groupBookStocks() {
    final Map<String, int> groupedStock = {};
    for (var stock in book.bookStocks) {
      if (stock.state == 'InPlace') {
        groupedStock[stock.location] = (groupedStock[stock.location] ?? 0) + 1;
      }
    }
    return groupedStock;
  }

  Widget _buildInfoSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'By ${book.authors.map((a) => a.name).join(', ')}',
          style: GoogleFonts.poppins(
            fontSize: 18,
            fontWeight: FontWeight.w500,
            color: Colors.white70,
          ),
        ),
        const SizedBox(height: 2),
        Text(
          'ISBN: ${book.isbn}',
          style: GoogleFonts.poppins(
            fontSize: 14,
            color: Colors.white70,
            fontStyle: FontStyle.italic,
          ),
        ),
        const SizedBox(height: 16),
        if (book.categories.isNotEmpty)
          Wrap(
            spacing: 8.0,
            runSpacing: 4.0,
            children: book.categories
                .map(
                  (c) => Chip(
                    label: Text(c.name),
                    backgroundColor: Colors.deepPurpleAccent.withOpacity(0.2),
                    shape: RoundedRectangleBorder(
                      side: const BorderSide(
                        color: Colors.deepPurpleAccent,
                        width: 1,
                      ),
                      borderRadius: BorderRadius.circular(20.0),
                    ),
                  ),
                )
                .toList(),
          ),
      ],
    );
  }

  Widget _buildDescriptionSection(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Description',
          style: Theme.of(context).textTheme.headlineSmall?.copyWith(
            fontWeight: FontWeight.bold,
            color: Colors.white,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          book.description,
          style: GoogleFonts.poppins(fontSize: 16, color: Colors.white70),
        ),
      ],
    );
  }

  Widget _buildStockSection(
    BuildContext context,
    Map<String, int> groupedStock,
  ) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Availability',
          style: Theme.of(context).textTheme.headlineSmall?.copyWith(
            fontWeight: FontWeight.bold,
            color: Colors.white,
          ),
        ),
        const SizedBox(height: 8),
        if (groupedStock.isEmpty)
          const Text('This book is currently not in stock.')
        else
          ...groupedStock.entries.map(
            (entry) => Card(
              margin: const EdgeInsets.symmetric(vertical: 4.0),
              child: ListTile(
                title: Text(entry.key),
                trailing: Text(
                  'Quantity: ${entry.value}',
                  style: const TextStyle(fontWeight: FontWeight.bold),
                ),
              ),
            ),
          ),
      ],
    );
  }
}
