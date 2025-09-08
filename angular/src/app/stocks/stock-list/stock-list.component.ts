import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StockAggregateService } from '../../proxy/stocks/stock-aggregate.service';
import type { StockSummaryDto } from '../../proxy/stocks/dtos/models';

@Component({
  selector: 'app-stock-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './stock-list.component.html',
  styleUrls: ['./stock-list.component.css'],
})
export class StockListComponent {
  private stockService = inject(StockAggregateService);
  private router = inject(Router);

  items = signal<StockSummaryDto[]>([]);
  total = signal(0);
  loading = signal(false);
  private searchTimer: any;

  filterText = '';
  pageSize = 10;
  pageIndex = 1;

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.stockService
      .getList({
        skipCount: (this.pageIndex - 1) * this.pageSize,
        maxResultCount: this.pageSize,
        sorting: 'CreationTime desc',
      })
      .subscribe({
        next: res => {
          const filter = (this.filterText || '').trim().toLowerCase();
          const filtered = filter
            ? res.items.filter(i => (i.name || '').toLowerCase().includes(filter))
            : res.items;
          this.items.set(filtered);
          this.total.set(filtered.length);
          this.loading.set(false);
        },
        error: _ => this.loading.set(false),
      });
  }

  onSearch() {
    this.pageIndex = 1;
    this.load();
  }

  onFilterChange(_: string) {
    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }
    this.searchTimer = setTimeout(() => {
      this.onSearch();
    }, 300);
  }

  onPageChange(dir: 'prev' | 'next') {
    const lp = this.lastPage();
    if (dir === 'prev' && this.pageIndex > 1) this.pageIndex--;
    if (dir === 'next' && this.pageIndex < lp) this.pageIndex++;
    this.load();
  }

  create() {
    this.router.navigate(['/stocks/create']);
  }

  lastPage(): number {
    return Math.max(1, Math.ceil(this.total() / this.pageSize));
  }

  edit(item: StockSummaryDto) {
    this.router.navigate(['/stocks', item.id, 'edit']);
  }

  remove(item: StockSummaryDto) {
    if (!confirm('Delete this stock record?')) return;
    this.loading.set(true);
    this.stockService.delete(item.id).subscribe({
      next: () => this.load(),
      error: _ => this.loading.set(false),
    });
  }
}
