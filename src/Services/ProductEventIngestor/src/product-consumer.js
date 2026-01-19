const { Client } = require('@elastic/elasticsearch');

// Cấu hình ElasticSearch
const esClient = new Client({ node: process.env.ES_NODE || 'http://localhost:9200' });

// Hàm tạo dữ liệu giả lập (Fake Data)
function generateFakeProducts(count) {
  const categories = ['Laptop', 'Smartphone', 'Tablet', 'Accessories'];
  const brands = ['Dell', 'Apple', 'Samsung', 'Sony', 'Asus'];
  
  return Array.from({ length: count }, () => {
    const id = Math.floor(Math.random() * 10000);
    return {
      id: `PROD-${id}`,
      name: `${brands[id % 5]} ${categories[id % 4]} Gen ${id % 10}`,
      price: Math.floor(Math.random() * 2000) + 100,
      category: categories[id % 4],
      description: 'Dữ liệu giả lập để test pipeline Read Side',
      stock_count: Math.floor(Math.random() * 100),
      created_at: new Date().toISOString()
    };
  });
}

// 2. Hàm thực hiện Bulk Index vào Elasticsearch
async function syncToElasticsearch() {
  const products = generateFakeProducts(100000); // Mỗi lần fake 100 sản phẩm
  console.log(`[Fake Pipeline] Đang chuẩn bị index ${products.length} sản phẩm...`);

  // Định dạng dữ liệu cho Bulk API
  const operations = products.flatMap(doc => [
    { index: { _index: 'products', _id: doc.id } },
    doc
  ]);

  try {
    const result = await esClient.bulk({ refresh: true, operations });
    return {
      success: !result.errors,
      count: products.length,
      took: result.took
    };
  } catch (err) {
    throw new Error(`Elasticsearch Bulk Error: ${err.message}`);
  }
}

// Export các hàm cần thiết
module.exports = {
  syncToElasticsearch
};