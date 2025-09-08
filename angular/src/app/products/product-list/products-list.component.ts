import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ProductService } from 'src/app/proxy/products';
import { ProductDto } from 'src/app/proxy/products/dtos';
import { ToasterService } from '@abp/ng.theme.shared';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-products-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './products-list.component.html',
  styleUrls: ['./products-list.component.css']
})
export class ProductsListComponent implements OnInit, OnDestroy {
  private service = inject(ProductService);
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private destroy$ = new Subject<void>();
  private search$ = new Subject<string>();

  public Math = Math;

  items: ProductDto[] = [];
  totalCount = 0;
  skipCount = 0;
  pageSize = 10;
  loading = false;

  categories: string[] = [];

  allCategories: string[] = [];
  private allCategoriesLoaded = false;
  filters: any = { filterText: '', category: '', status: '' };
  

  ngOnInit(): void {
    this.search$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(term => {
        this.filters.filterText = term ?? '';
        this.skipCount = 0;
        this.reload();
      });

    this.reload();
    this.loadAllCategories();
  }

  reload() {
    this.loading = true;
    this.service.getList({
      filterText: this.filters.filterText,
      category: this.filters.category || undefined,
      status: this.filters.status === '' ? undefined : Number(this.filters.status),
      skipCount: this.skipCount,
      maxResultCount: this.pageSize,
      sorting: 'CreationTime desc'
    }).subscribe({
      next: res => {
        this.items = res.items;
        this.totalCount = res.totalCount;
        this.categories = Array.from(new Set(res.items.map(i => i.category).filter(Boolean) as string[]));
        const merged = new Set<string>([...this.allCategories, ...this.categories]);
        this.allCategories = Array.from(merged).sort((a, b) => a.localeCompare(b));
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

  onSearchChange(term: string){
    this.search$.next(term ?? '');
  }

  onCategoryChange(category: string){
    this.filters.category = category ?? '';
    this.skipCount = 0;
    this.reload();
  }

  onStatusChange(status: string){
    this.filters.status = status ?? '';
    this.skipCount = 0;
    this.reload();
  }

  private loadAllCategories(): void {
    if (this.allCategoriesLoaded) {
      return;
    }
    this.service.getList({
      filterText: undefined as any,
      category: undefined as any,
      skipCount: 0,
      maxResultCount: 1000,
    }).subscribe({
      next: res => {
        const cats = Array.from(new Set((res.items || []).map(i => i.category).filter(Boolean) as string[]));
        if (cats.length) {
          this.allCategories = Array.from(new Set([...(this.allCategories || []), ...cats])).sort((a, b) => a.localeCompare(b));
          this.allCategoriesLoaded = true;
        }
      },
  
    });
  }

  onCategoryFocus(): void {
    this.loadAllCategories();
  }

  viewProduct(id: string): void {
    this.router.navigate(['/products', id]);
  }

  

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
