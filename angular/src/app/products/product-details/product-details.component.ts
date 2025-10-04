import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ProductService } from 'src/app/proxy/products';
import { ProductDto, ProductVariantOptionDto } from 'src/app/proxy/products/dtos';
import { ToasterService } from '@abp/ng.theme.shared';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './product-details.component.html',
  styleUrls: ['./product-details.component.css']
})
export class ProductDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private service = inject(ProductService);
  private toaster = inject(ToasterService);

  product: ProductDto | null = null;
  loading = false;
  id: string | null = null;
  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id');
    if (this.id) {
      this.loadProduct();
    }
  }

  private loadProduct(): void {
    if (!this.id) return;
    
    this.loading = true;
    this.service.get(this.id).subscribe({
      next: (product) => {
        this.product = product;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.toaster.error('Failed to load product details');
        console.error(err);
      }
    });
  }

  private formatOptions(options?: ProductVariantOptionDto[] | null): string {
    const list = options ?? [];
    return list.map(o => `${o.name}: ${o.value}`).join(', ');
  }

  private confirmDelete(): void {
    if (!this.product || !confirm(`Delete product "${this.product.name}"?`)) return;
    
    this.loading = true;
    this.service.delete(this.product.id).subscribe({
      next: () => {
        this.toaster.success('Product deleted successfully');
        this.router.navigate(['/products']);
      },
      error: (err) => {
        this.loading = false;
        this.toaster.error('Delete failed');
        console.error(err);
      }
    });
  }
}
