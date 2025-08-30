import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ProductService } from 'src/app/proxy/products';
import { ProductDto } from 'src/app/proxy/products/dtos';
import { ToasterService } from '@abp/ng.theme.shared';

@Component({
  selector: 'app-products-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './products-list.component.html',
  styleUrls: ['./products-list.component.css']
})
export class ProductsListComponent implements OnInit {
  private service = inject(ProductService);
  private router = inject(Router);
  private toaster = inject(ToasterService);

  public Math = Math;

  items: ProductDto[] = [];
  totalCount = 0;
  skipCount = 0;
  pageSize = 10;
  loading = false;

  categories: string[] = [];
  filters: any = { filterText: '', category: '' };

  ngOnInit(): void {
    this.reload();
  }

  reload() {
    this.loading = true;
    this.service.getList({
      filterText: this.filters.filterText,
      category: this.filters.category || undefined,
      skipCount: this.skipCount,
      maxResultCount: this.pageSize,
    }).subscribe({
      next: res => {
        this.items = res.items;
        this.totalCount = res.totalCount;
        this.categories = Array.from(new Set(res.items.map(i => i.category).filter(Boolean) as string[]));
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.toaster.error('Failed to load products');
        console.error(err);
      }
    });
  }

  next(){ this.skipCount = Math.min(this.skipCount + this.pageSize, Math.max(0, this.totalCount - this.pageSize)); this.reload(); }
  prev(){ this.skipCount = Math.max(0, this.skipCount - this.pageSize); this.reload(); }

  confirmDelete(p: ProductDto){
    if (!confirm(`Delete product "${p.name}"?`)) return;
    this.loading = true;
    this.service.delete(p.id).subscribe({
      next: () => { this.toaster.success('Deleted'); this.reload(); },
      error: err => { this.loading = false; this.toaster.error('Delete failed'); console.error(err); }
    })
  }
}
